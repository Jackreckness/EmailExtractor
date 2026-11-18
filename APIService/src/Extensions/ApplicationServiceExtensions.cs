using EmailExtractor.Endpoints;
using EmailExtractor.Services;

namespace EmailExtractor.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IExtractionRulesService, ExtractionRulesService>();
        services.AddSingleton<IEndpoint, ExtractEmailEndpoint>();
        services.AddScoped<IEmailExtractorService, EmailExtractorService>();
        return services;
    }
}
