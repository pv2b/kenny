using System.Text.Json.Serialization;
using System.Text.Json;
using PmpApiClient;

public class ApiKeyring {
    private class Item {
        public string? ApiBaseUri { get; set; }
        public string? ApiAuthToken { get; set; }
        public List<string> AllowGroups { get; set; }
        public List<string> DenyGroups { get; set; }
        public List<string> AllowUsers { get; set; }
        public List<string> DenyUsers { get; set; }
        public Item() {
            AllowGroups = new List<string>();
            DenyGroups = new List<string>();
            AllowUsers = new List<string>();
            DenyUsers = new List<string>();
        }
    }
    private Dictionary<string, Item> _keyring;

    public ApiKeyring(IConfiguration configuration) {
        _keyring = new Dictionary<string, Item>();
        configuration.GetSection("PmpApi").Bind(_keyring);
    }

    public BasePmpApiClient CreateApiClient(string collection, CancellationToken cancellationToken = default) {
        Item item = _keyring[collection];
        
        if (item.ApiBaseUri == null)
            throw new Exception($"ApiBaseUri is not set for {collection}");
        else if (item.ApiAuthToken == null)
            throw new Exception($"ApiAuthToken is not set for {collection}");
        return new PmpApiClient.PmpApiClient(new Uri(item.ApiBaseUri), item.ApiAuthToken, cancellationToken);
    }

    public IEnumerable<string> GetCollectionNames() {
        return _keyring.Keys;
    }

    public IEnumerable<string> GetAllowGroups(string collection) {
        return _keyring[collection].AllowGroups;
    }

    public IEnumerable<string>? GetDenyGroups(string collection) {
        return _keyring[collection].DenyGroups;
    }

    public IEnumerable<string>? GetAllowUsers(string collection) {
        return _keyring[collection].AllowUsers;
    }

    public IEnumerable<string>? GetDenyUsers(string collection) {
        return _keyring[collection].DenyUsers;
    }
}