using EmailExtractor.Endpoints;
using EmailExtractor.Extensions;
using EmailExtractor.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddValidation();

var app = builder.Build();
// Configure global exception handling
app.UseExceptionHandler(errors => errors.Run(ApiExceptionHandler.Handle));
// Configure Endpoints
app.MapEndpointsMapper();

app.Run();
