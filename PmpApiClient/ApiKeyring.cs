using System.Text.Json.Serialization;
using System.Text.Json;

namespace PmpApiClient;

public class ApiKeyring {
    private class Item {
        public string ApiBaseUri { get; }
        public string ApiAuthToken { get; }
        public List<string>? AllowGroups { get; }
        public List<string>? DenyGroups { get; }
        public List<string>? AllowUsers { get; }
        public List<string>? DenyUsers { get; }
        public Item(string apiBaseUri, string apiAuthToken,
                    List<string>? allowGroups, List<string>? denyGroups,
                    List<string>? allowUsers, List<string>? denyUsers) {
            ApiBaseUri = apiBaseUri;
            ApiAuthToken = apiAuthToken;
            AllowGroups = allowGroups;
            DenyGroups = denyGroups;
            AllowUsers = allowUsers;
            DenyUsers = denyUsers;
        }
    }
    private Dictionary<string, Item> _keyring;

    public ApiKeyring(String filename) {
        using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read)) {
            _keyring = JsonSerializer.Deserialize<Dictionary<string, Item>>(fs) ?? new Dictionary<string, Item>();
        }
        foreach (var item in _keyring)
            if (item.Value.ApiBaseUri == null)
                throw new Exception($"ApiBaseUri is not set for {item.Key}");
            else if (item.Value.ApiAuthToken == null)
                throw new Exception($"ApiAuthToken is not set for {item.Key}");
    }

    public BasePmpApiClient CreateApiClient(string collection, CancellationToken cancellationToken = default) {
        Item item = _keyring[collection];
        
        return new PmpApiClient(new Uri(item.ApiBaseUri), item.ApiAuthToken, cancellationToken);
    }

    public IEnumerable<string> GetCollectionNames() {
        return _keyring.Keys;
    }

    public IEnumerable<string> GetAllowGroups(string collection) {
        return _keyring[collection].AllowGroups ?? Enumerable.Empty<string>();
    }

    public IEnumerable<string>? GetDenyGroups(string collection) {
        return _keyring[collection].DenyGroups ?? Enumerable.Empty<string>();
    }

    public IEnumerable<string>? GetAllowUsers(string collection) {
        return _keyring[collection].AllowUsers ?? Enumerable.Empty<string>();
    }

    public IEnumerable<string>? GetDenyUsers(string collection) {
        return _keyring[collection].DenyUsers ?? Enumerable.Empty<string>();
    }
}