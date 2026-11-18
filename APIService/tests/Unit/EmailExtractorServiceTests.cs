using EmailExtractor.Configuration;
using EmailExtractor.Services;
using EmailExtractor.Utils;
using NUnit.Framework;

namespace EmailExtractor.Tests.Unit;

[TestFixture]
[Category("Unit")]
public class EmailExtractorServiceTests
{
    [Test]
    public void Parse_ExtractsTagNamesWithDigitsSpacesAndPunctuation()
    {
        var result = CreateService().Parse(
            "<123 tag!>value</123 tag!><path/name>other</path/name><total>115</total>");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExtractedFields["123 tag!"], Is.EqualTo("value"));
            Assert.That(result.ExtractedFields["path/name"], Is.EqualTo("other"));
        });
    }

    [Test]
    public void Parse_ExtractsInlineFieldsWithDifferentValues()
    {
        var result = CreateService().Parse(
            "<vendor>City Books</vendor><reservation_date>18 November</reservation_date><total>115.50</total>");

        Assert.Multiple(() =>
        {
            Assert.That(result.ExtractedFields["vendor"], Is.EqualTo("City Books"));
            Assert.That(result.ExtractedFields["reservationDate"], Is.EqualTo("18 November"));
            Assert.That(result.ExtractedFields["total"], Is.EqualTo("115.50"));
        });
    }

    [Test]
    public void Parse_PreservesRepeatedSiblingValuesInOrder()
    {
        var result = CreateService().Parse(
            "<expense><total>115</total><payment_method>card</payment_method><payment_method>cash</payment_method></expense>");
        var expense = (Dictionary<string, object>)result.ExtractedFields["expense"];

        Assert.That(expense["paymentMethod"], Is.EqualTo(new[] { "card", "cash" }));
    }

    [Test]
    public void Parse_SumsTotalsAcrossDifferentParentTags()
    {
        var result = CreateService().Parse(
            "<invoice><total>115</total></invoice><order><details><total>230</total></details></order>");
        var invoice = (Dictionary<string, object>)result.ExtractedFields["invoice"];
        var order = (Dictionary<string, object>)result.ExtractedFields["order"];
        var details = (Dictionary<string, object>)order["details"];

        Assert.Multiple(() =>
        {
            Assert.That(invoice["total"], Is.EqualTo("115"));
            Assert.That(details["total"], Is.EqualTo("230"));
            Assert.That(result.Calculation.Total, Is.EqualTo(345m));
            Assert.That(result.Calculation.SalesTax, Is.EqualTo(45m));
            Assert.That(result.Calculation.TotalExcludingTax, Is.EqualTo(300m));
        });
    }

    [Test]
    public void Parse_RejectsEmailWhenTotalIsMissing()
    {
        var error = Assert.Throws<ApiException>(() => CreateService().Parse("<vendor>Shop</vendor>"));

        Assert.That(error!.Error, Is.EqualTo(ErrorCodes.MissingTotal));
    }

    [Test]
    public void Parse_DefaultsMissingCostCentreToUnknown()
    {
        var result = CreateService().Parse("<expense><total>115</total></expense>");
        var expense = (Dictionary<string, object>)result.ExtractedFields["expense"];

        Assert.That(expense["costCentre"], Is.EqualTo("UNKNOWN"));
        Assert.That(result.Warnings, Is.EqualTo(new[] { "expense.costCentre was missing. Used default UNKNOWN." }));
    }

    [Test]
    public void Parse_DoesNotWarnWhenTheDefaultValueWasProvided()
    {
        var result = CreateService().Parse("<expense><total>115</total><cost_centre>UNKNOWN</cost_centre></expense>");

        Assert.That(result.Warnings, Is.Empty);
    }

    [TestCase("")]
    [TestCase(" \t\r\n ")]
    public void Parse_DefaultsBlankCostCentreAndWarns(string value)
    {
        var result = CreateService().Parse($"<expense><total>115</total><cost_centre>{value}</cost_centre></expense>");
        var expense = (Dictionary<string, object>)result.ExtractedFields["expense"];

        Assert.That(expense["costCentre"], Is.EqualTo("UNKNOWN"));
        Assert.That(result.Warnings, Is.EqualTo(new[] { "expense.costCentre was empty. Used default UNKNOWN." }));
    }

    [Test]
    public void Parse_DefaultsOnlyBlankValuesAmongRepeatedFields()
    {
        var result = CreateService().Parse(
            "<expense><total>115</total><cost_centre></cost_centre><cost_centre>DEV632</cost_centre></expense>");
        var expense = (Dictionary<string, object>)result.ExtractedFields["expense"];

        Assert.That(expense["costCentre"], Is.EqualTo(new[] { "UNKNOWN", "DEV632" }));
        Assert.That(result.Warnings, Has.Count.EqualTo(1));
    }

    private static EmailExtractorService CreateService()
    {
        var rules = new ExtractionRules { TaxRate = 0.15m };
        rules.DefaultsByParent["expense"] = new Dictionary<string, string> { ["costCentre"] = "UNKNOWN" };
        return new EmailExtractorService(new TestRulesService(rules));
    }

    private class TestRulesService(ExtractionRules rules) : IExtractionRulesService
    {
        public ExtractionRules Rules { get; } = rules;
    }
}
