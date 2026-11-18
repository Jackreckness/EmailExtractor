using EmailExtractor.Contracts;

namespace EmailExtractor.Services;

public interface IEmailExtractorService
{
    ExtractorResponse Parse(string text);
}
