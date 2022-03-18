using System.Text.Json.Serialization;

namespace PmpApiClient;

public class AssociatedGroupContainer {
    [JsonPropertyName("ASSOCIATED GROUPS")]
    public IEnumerable<ResourceGroupSummary>? Groups { get; set; }
}