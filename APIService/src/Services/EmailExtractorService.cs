using System.Globalization;
using EmailExtractor.Configuration;
using EmailExtractor.Contracts;
using EmailExtractor.Models;
using EmailExtractor.Utils;

namespace EmailExtractor.Services;

/// <summary>
/// Extracts data from the markup text and returns a JSON object with the extracted fields, 
/// as well as a calculation result and any warnings.
/// </summary>
/// <param name="rulesService"></param>
public class EmailExtractorService(IExtractionRulesService rulesService) : IEmailExtractorService
{
    private readonly ExtractionRules rules = rulesService.Rules;

    public ExtractorResponse Parse(string text)
    {
        // Parse the text and get a tree of XMLMarkup objects
        var xmlMarkups = ParseMarkup(text);
        var totals = FindTotals(xmlMarkups).ToArray();
        if (totals.Length == 0)
        {
            throw new ApiException(ErrorCodes.MissingTotal);
        }

        decimal total = 0.0m;
        try
        {
            foreach (var totalTag in totals)
            {
                decimal value = decimal.Parse(totalTag.Value, CultureInfo.InvariantCulture);
                if (value <= 0)
                {
                    throw new ApiException(ErrorCodes.InvalidTotal);
                }
                total += value;     
            }
        }
        catch (Exception)
        {
            throw new ApiException(ErrorCodes.InvalidTotal);
        }

        var (salesTax, totalExcludingTax) = TaxCalculator.Calculate(total, rules.TaxRate);
        var root = new XMLMarkup("", "", xmlMarkups);
        var warnings = new List<string>();
        var jsonObject = BuildJsonObject(root, warnings);
        return new ExtractorResponse(jsonObject, new CalculationResult(total, salesTax, totalExcludingTax, rules.TaxRate), warnings);
    }

    private IList<XMLMarkup> ParseMarkup(string text)
    {
        var roots = new List<XMLMarkup>();

        // Stack to keep track of open tags and their corresponding text parts
        var openTags = new Stack<(XMLMarkup Markup, List<string> TextParts)>();
        var previousEnd = 0;

        foreach (var tag in TagDetection.FindTags(text))
        {
            if (tag.Index > previousEnd && openTags.Count > 0)
            {
                // Add the text between the previous tag and the current tag to the text parts of the last open tag
                openTags.Peek().TextParts.Add(text[previousEnd..tag.Index]);
            }

            previousEnd = tag.Index + tag.Length;
            var tagName = tag.Groups["name"].Value;
            var isClosing = tagName.StartsWith('/');
            if (!isClosing)
            {
                openTags.Push((new XMLMarkup(tagName, ""), []));
                continue;
            }

            tagName = tagName[1..];
            if (openTags.Count == 0)
            {
                throw new ApiException(ErrorCodes.MissingOpeningTag(tagName));
            }

            var opening = openTags.Pop();
            if (opening.Markup.TagName != tagName)
            {
                var error = openTags.Any(openTag => openTag.Markup.TagName == tagName)
                    ? ErrorCodes.MismatchedClosingTag(opening.Markup.TagName, tagName)
                    : ErrorCodes.MissingOpeningTag(tagName);
                throw new ApiException(error);
            }

            opening.Markup.Value = string.Concat(opening.TextParts).Trim();
            if (openTags.Count > 0)
            {
                openTags.Peek().Markup.Children.Add(opening.Markup);
            }
            else
            {
                roots.Add(opening.Markup);
            }
        }

        if (openTags.Count > 0)
        {
            throw new ApiException(ErrorCodes.MissingClosingTag(openTags.Peek().Markup.TagName));
        }

        return roots;
    }

    private static IEnumerable<XMLMarkup> FindTotals(IEnumerable<XMLMarkup> tags) =>
        tags.SelectMany(tag => tag.IsTotal ? [tag] : FindTotals(tag.Children));

    private Dictionary<string, object> BuildJsonObject(XMLMarkup parent, List<string> warnings)
    {
        var jsonObject = new Dictionary<string, object>();
        
        var defaults = rules.DefaultsByParent.GetValueOrDefault(parent.TagName);
        foreach (var group in parent.Children.GroupBy(tag => JsonFieldName.From(tag.TagName)))
        {
            var values = group.Select(BuildValue).ToArray();
            jsonObject.Add(group.Key, values.Length == 1 ? values[0] : values);
        }

        // Add default values for missing fields, if any
        if (defaults is not null)
        {
            foreach (var (key, defaultValue) in defaults)
            {
                if (jsonObject.TryAdd(key, defaultValue))
                {
                    var fieldName = parent.TagName.Length > 0 ? $"{JsonFieldName.From(parent.TagName)}.{key}" : key;
                    warnings.Add($"{fieldName} was missing. Used default {defaultValue}.");
                }
            }
        }

        return jsonObject;

        object BuildValue(XMLMarkup tag)
        {
            if (tag.Children.Count > 0
                || (tag.Value.Length == 0 && rules.DefaultsByParent.ContainsKey(tag.TagName)))
            {
                return BuildJsonObject(tag, warnings);
            }

            var key = JsonFieldName.From(tag.TagName);
            if (string.IsNullOrWhiteSpace(tag.Value) && defaults?.ContainsKey(key) == true)
            {
                var defaultValue = defaults[key];
                var fieldName = parent.TagName.Length > 0 ? $"{JsonFieldName.From(parent.TagName)}.{key}" : key;
                warnings.Add($"{fieldName} was empty. Used default {defaultValue}.");
                return defaultValue;
            }

            return tag.Value;
        }
    }
}
