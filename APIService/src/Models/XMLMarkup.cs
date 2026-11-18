namespace EmailExtractor.Models;

/// <summary>
/// A simple model representing an XML markup elements.
/// </summary>
public record XMLMarkup
{
    public XMLMarkup(string tagName, string value, IList<XMLMarkup> children)
    {
        TagName = tagName;
        Value = value;
        Children = children;
    }

    public XMLMarkup(string tagName, string value) : this(tagName, value, [])
    {
    }

    public string TagName { get; }
    
    public string Value { get; set; }

    public IList<XMLMarkup> Children { get; }

    public bool IsTotal => TagName == "total";
}
