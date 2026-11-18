namespace EmailExtractor.Utils;

public record ErrorCode(int StatusCode, string Message);


/// <summary>
/// Predefined error codes and messages. 
/// </summary>
public static class ErrorCodes
{
    public static readonly ErrorCode MissingTotal  = BadRequest("The email must contain a <total> tag.");
    public static readonly ErrorCode InvalidTotal = BadRequest("The <total> value must be a positive number, such as 35000 or 35,000.00.");

    public static ErrorCode MissingOpeningTag(string name) => BadRequest($"Missing opening tag <{name}> for </{name}>.");

    public static ErrorCode MissingClosingTag(string name) => BadRequest($"Missing closing tag </{name}>.");

    public static ErrorCode MismatchedClosingTag(string expected, string actual) => BadRequest($"Missing closing tag </{expected}> before </{actual}>.");

    private static ErrorCode BadRequest(string message) => new(StatusCodes.Status400BadRequest, message);
}
