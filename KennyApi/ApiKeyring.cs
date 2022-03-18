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

    private bool IsAuthorizedUser(ClaimsPrincipal user, Item item) {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
            return false;

        if (user.Identity.Name == null)
            return false;

        string username = user.Identity.Name;

        bool UserIsInList(IEnumerable<string>? userList) {
            if (userList == null)
                return false;
            foreach (string listedUser in userList)
                if (String.Equals(username, listedUser, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        bool UserIsInAnyListedRole(IEnumerable<string>? roleList) {
            if (roleList == null)
                return false;
            foreach (string listedRole in roleList)
                if (user.IsInRole(listedRole))
                    return true;
            return false;
        }

        if (UserIsInList(item.DenyUsers) || UserIsInAnyListedRole(item.DenyGroups))
            return false;

        return UserIsInList(item.AllowUsers) || UserIsInAnyListedRole(item.AllowGroups);
    }

    public BasePmpApiClient GetApiClient(ClaimsPrincipal user, String collection) {
        Item item = _keyring[collection];
        
        if (!IsAuthorizedUser(user, item))
            throw new UnauthorizedAccessException();
        return new PmpApiClient.PmpApiClient(new Uri(item.ApiBaseUri), item.ApiAuthToken);
    }
}