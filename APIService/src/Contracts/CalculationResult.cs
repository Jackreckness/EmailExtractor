namespace EmailExtractor.Contracts;

/// <summary>
/// Represents the result of a calculation, including the total amount, sales tax, total excluding tax, and tax rate.
/// </summary>
public class CalculationResult
{
    public CalculationResult(decimal total, decimal salesTax, decimal totalExcludingTax, decimal taxRate)
    {
        Total = total;
        SalesTax = salesTax;
        TotalExcludingTax = totalExcludingTax;
        TaxRate = taxRate;
    }

    public decimal Total { get; }
    public decimal SalesTax { get; }
    public decimal TotalExcludingTax { get; }
    public decimal TaxRate { get; }
}
