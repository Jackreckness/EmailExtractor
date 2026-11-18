using EmailExtractor.Contracts;
using EmailExtractor.Services;

namespace EmailExtractor.Endpoints;

/// <summary>
/// Defines an endpoint for extracting data from a given text using the EmailExtractorService.
/// </summary>
internal class ExtractEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/extract", (ExtractRequest request, IEmailExtractorService extractor) => {
            return Results.Ok(extractor.Parse(request.Text));
        });
    }
}
