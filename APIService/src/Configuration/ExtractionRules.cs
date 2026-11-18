namespace EmailExtractor.Configuration;

/// <summary>
/// The extraction rules used to parse the markup text and extract the fields and calculation results.
/// </summary>
public class ExtractionRules
{
    public decimal TaxRate { get; set; } = 0;
    public Dictionary<string, Dictionary<string, string>> DefaultsByParent { get; } = [];
}
