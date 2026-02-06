namespace AgroSolutions.Ingestion.Service.Domain.Exceptions.Responses;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }

    public ErrorResponse(int statusCode, string message, Dictionary<string, string[]>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }
}
