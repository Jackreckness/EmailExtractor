namespace EmailExtractor.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder endpoints);
}

/// <summary>
/// A simple extension method for mapping all endpoints in the application.
/// </summary>
public static class Endpoints
{
    public static void MapEndpointsMapper(this WebApplication app)
    {
        foreach (var endpoint in app.Services.GetServices<IEndpoint>())
        {
            endpoint.MapEndpoint(app);
        }
    }
}
