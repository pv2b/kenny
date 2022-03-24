using System.Text.Json.Serialization;

namespace PmpApiClient;

public class ResourceGroupSummary {
    [JsonPropertyName("GROUP ID")]
    public int? Id { get; set; }

    [JsonPropertyName("GROUP NAME")]
    public string? Name { get; set; }
}