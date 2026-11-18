namespace EmailExtractor.Utils;

/// <summary>
/// Provides utility methods for calculating sales tax and total excluding tax based on a given tax rate.
/// </summary>
public static class TaxCalculator
{
    public static (decimal SalesTax, decimal TotalExcludingTax) Calculate(decimal total, decimal rate)
    {
        var taxFraction = rate / (1 + rate);
        var salesTax = Math.Round(total * taxFraction, 2);
        return (salesTax, total - salesTax);
    }
}
