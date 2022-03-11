using System.Text.Json.Serialization;

namespace PmpApiClient;
public class ApiResult
{
    public ApiResult(string status, string message) {
        Status = status;
        Message = message;
    }

    [JsonPropertyName("status")]
    public string Status { get; }

    [JsonPropertyName("message")]
    public string Message { get; }
}
