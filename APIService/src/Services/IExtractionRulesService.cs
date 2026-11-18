using EmailExtractor.Configuration;

namespace EmailExtractor.Services;

public interface IExtractionRulesService
{
    ExtractionRules Rules { get; }
}
