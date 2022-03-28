using System.Text.Json.Serialization;

namespace PmpApiClient;

public class ResourceGroupSummary {
    [JsonPropertyName("GROUP ID")]
    public long Id { get; set; } = -1;

    [JsonPropertyName("GROUP NAME")]
    public string? Name { get; set; }
}