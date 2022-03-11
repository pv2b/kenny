using System.Text.Json.Serialization;

namespace PmpApiClient;
public class ApiOperation<T>
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