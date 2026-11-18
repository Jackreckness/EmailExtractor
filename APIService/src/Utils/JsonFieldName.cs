using System.Text;

namespace EmailExtractor.Utils;

/// <summary>
/// Provides utility methods for converting JSON field names
/// </summary>
public static class JsonFieldName
{
    // Converts a string to camelCase from snake_case
    public static string From(string name)
    {
        var words = name.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return name;
        }

        var result = new StringBuilder(words[0].ToLowerInvariant());
        foreach (var word in words.Skip(1))
        {
            var camel = word.ToLowerInvariant();
            result.Append(char.ToUpperInvariant(camel[0]));
            result.Append(camel.AsSpan(1));
        }

        return result.ToString();
    }
}
