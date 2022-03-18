using System.Text.Json.Serialization;

namespace PmpApiClient;

public class ResourceSummary {
    public ResourceSummary(string description, string name, string id, string type, int noOfAccounts) {
        Description = description;
        Name = name;
        Id = id;
        Type = type;
        NoOfAccounts = noOfAccounts;
    }

    [JsonPropertyName("RESOURCE DESCRIPTION")]
    public string Description { get; }

    [JsonPropertyName("RESOURCE NAME")]
    public string Name { get; }

    [JsonPropertyName("RESOURCE ID")]
    public string Id { get; }

    [JsonPropertyName("RESOURCE TYPE")]
    public string Type { get; }

    [JsonPropertyName("NOOFACCOUNTS")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int NoOfAccounts { get; }
}
