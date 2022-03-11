using System.Text.Json.Serialization;

namespace PmpApiClient;

public class ApiResponse<T>
{
    public class ApiOperation
    {
        public ApiOperation(string name, ApiResult result, int totalRows, T details) {
            Name = name;
            Result = result;
            TotalRows = totalRows;
            Details = details;
        }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("result")]
        public ApiResult Result { get; }

        [JsonPropertyName("totalRows")]
        public int TotalRows { get; }

        [JsonPropertyName("Details")]
        public T Details { get; }
    }

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

    public ApiResponse(ApiOperation operation) {
        Operation = operation;
    }
    [JsonPropertyName("operation")]
    public ApiOperation Operation { get; }
}