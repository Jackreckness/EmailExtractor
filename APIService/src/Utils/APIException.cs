namespace EmailExtractor.Utils;

/// <summary>
/// A simple wrapper for an exception that can be thrown when an API error occurs. 
/// </summary>
/// <param name="error"></param>
public class ApiException(ErrorCode error) : Exception(error.Message)
{
    public ErrorCode Error { get; } = error;
}