using System.Text.Json.Serialization;

namespace PmpApiClient;
public class ApiResponse<T>
{
    public ApiResponse(ApiOperation<T> operation) {
        Operation = operation;
    }
    [JsonPropertyName("operation")]
    public ApiOperation<T> Operation { get; }
}