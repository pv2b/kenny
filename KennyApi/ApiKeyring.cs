using System.Text.Json.Serialization;
using System.Text.Json;
using PmpApiClient;
using System.Security.Claims;

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
        using (FileStream fs = File.Open(filename, FileMode.Open)) {
            _keyring = JsonSerializer.Deserialize<Dictionary<string, Item>>(fs) ?? new Dictionary<string, Item>();
        }
        foreach (var item in _keyring)
            if (item.Value.ApiBaseUri == null)
                throw new Exception($"ApiBaseUri is not set for {item.Key}");
            else if (item.Value.ApiAuthToken == null)
                throw new Exception($"ApiAuthToken is not set for {item.Key}");
    }

    public BasePmpApiClient GetApiClient(ClaimsPrincipal user, String collection) {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException();

        Item item = _keyring[collection];
        
        if (user.Identity.Name == null)
            throw new UnauthorizedAccessException();

        string username = user.Identity.Name;

        /* Check if user is in deny list */
        if (item.DenyUsers != null)
            foreach (string denyUser in item.DenyUsers)
                if (String.Equals(username, denyUser, StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException();

        /* Check if user is in deny group */
        if (item.DenyGroups != null)
            foreach (string denyGroup in item.DenyGroups)
                if (user.IsInRole(denyGroup))
                    throw new UnauthorizedAccessException();

        /* Check if user is in allow list */
        if (item.AllowUsers != null)
            foreach (string allowUser in item.AllowUsers)
                if (String.Equals(username, allowUser, StringComparison.OrdinalIgnoreCase))
                    goto authorized;

        /* Check if user is in allow group */
        if (item.AllowGroups != null)
            foreach (string allowGroup in item.AllowGroups)
                if (user.IsInRole(allowGroup))
                    goto authorized;

        /* User was not authorized in the allow list or group */
        throw new UnauthorizedAccessException();

authorized:
        /* loader ensures these aren't null */
        #pragma warning disable CS8604 
        return new PmpApiClient.PmpApiClient(new Uri(item.ApiBaseUri), item.ApiAuthToken);
    }
}