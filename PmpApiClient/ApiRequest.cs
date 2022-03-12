using System.Text.Json.Serialization;

namespace PmpApiClient;

public class ApiRequest<T>
{
    public class ApiOperation
    {
        public ApiOperation(T details) {
            Details = details;
        }

        [JsonPropertyName("Details")]
        public T Details { get; }
    }

    [JsonConstructor]
    public ApiRequest(ApiOperation operation) {
        Operation = operation;
    }

    public ApiRequest(T details) {
        Operation = new ApiOperation(details);
    }

    [JsonPropertyName("operation")]
    public ApiOperation Operation { get; }
}