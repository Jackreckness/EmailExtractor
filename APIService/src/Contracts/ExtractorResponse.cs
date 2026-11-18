namespace EmailExtractor.Contracts;

/// <summary>
/// Represents a response from the extractor service, containing the extracted fields, calculation results, and any warnings that may have occurred during processing.
/// </summary>
public class ExtractorResponse
{
    public ExtractorResponse(Dictionary<string, object> extractedFields, CalculationResult calculation, List<string> warnings)
    {
        ExtractedFields = extractedFields;
        Calculation = calculation;
        Warnings = warnings;
    }

    public Dictionary<string, object> ExtractedFields { get; }
    public CalculationResult Calculation { get; }
    public List<string> Warnings { get; }
}

