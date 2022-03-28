using System.Text.Json.Serialization;
using System.Text.Json;
using PmpApiClient;
using PmpSqlClient;
using System.Security.Claims;

public class ApiKeyring {
    private class Item {
        public string? ApiBaseUri { get; set; }
        public string? ApiAuthToken { get; set; }
        public string? ConnectionString { get; set; }
        public IEnumerable<ResourceGroupAce>? Acl { get; set; }
    }
    private Dictionary<string, Item> _keyring;
    private HttpClient _httpClient;

    public ApiKeyring(IHttpClientFactory httpClientFactory, IConfiguration configuration) {
        _keyring = new Dictionary<string, Item>();
        configuration.GetSection("PmpApi").Bind(_keyring);
        _httpClient = httpClientFactory.CreateClient();
    }

    public BasePmpApiClient CreateApiClient(string collection, CancellationToken cancellationToken = default) {
        Item item = _keyring[collection];
        
        if (item.ApiBaseUri == null)
            throw new Exception($"ApiBaseUri is not set for {collection}");
        else if (item.ApiAuthToken == null)
            throw new Exception($"ApiAuthToken is not set for {collection}");
        return new PmpApiClient.PmpApiClient(new Uri(item.ApiBaseUri), item.ApiAuthToken, _httpClient, cancellationToken);
    }

    public PmpSqlClient.PmpSqlClient CreateSqlClient(string collection) {
        Item item = _keyring[collection];
        
        if (item.ConnectionString == null)
            throw new Exception($"ConnectionString is not set for {collection}");
        return new PmpSqlClient.PmpSqlClient(item.ConnectionString);
    }

    public IEnumerable<string> GetCollectionNames() {
        return _keyring.Keys;
    }

    public IEnumerable<ResourceGroupAce> GetAcl(string collection) {
        return _keyring[collection].Acl ?? new List<ResourceGroupAce>();
    }
}