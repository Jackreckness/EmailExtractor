using System.ComponentModel.DataAnnotations;

namespace EmailExtractor.Contracts;

/// <summary>
/// Represents a request to extract data from a given text. The request must include the text to be processed.
/// </summary>
public class ExtractRequest
{
    public ExtractRequest(string text)
    {
        Text = text;
    }

    [Required]
    public string Text { get; }
}
