using System.ComponentModel.DataAnnotations;
using EmailExtractor.Utils;
using Microsoft.AspNetCore.Diagnostics;

namespace EmailExtractor.Handlers;

/// <summary>
/// An exception for unified handling of API errors. 
/// </summary>
public static class ApiExceptionHandler
{
    public static Task Handle(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ApiException)
        {
            var error = ((ApiException)exception).Error;
            return WriteError(context, error);
        } 
        else if (exception is ValidationException)
        {
            var error = new ErrorCode(StatusCodes.Status400BadRequest, exception.Message);
            return WriteError(context, error);
        }
        else
        {
            return WriteError(context, new ErrorCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred."));
        }
    }

    private static Task WriteError(HttpContext context, ErrorCode error)
    {
        context.Response.StatusCode = error.StatusCode;
        return context.Response.WriteAsJsonAsync(new { error.Message }, context.RequestAborted);
    }
}
