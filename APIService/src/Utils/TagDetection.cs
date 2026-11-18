using System.Text.RegularExpressions;

namespace EmailExtractor.Utils;

/// <summary>
/// Provides utility methods for detecting tags in a string of text.
/// </summary>
public static class TagDetection
{
    private static readonly Regex TagPattern = new(@"<(?<name>[^<>]+)>");

    public static IEnumerable<Match> FindTags(string text)
    {
        for (var match = TagPattern.Match(text); match.Success; match = match.NextMatch())
        {
            yield return match;
        }
    }
}
