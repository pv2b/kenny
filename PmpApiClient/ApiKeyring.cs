using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Claims;

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
        using (FileStream fs = File.Open(filename, FileMode.Open)) {
            _keyring = JsonSerializer.Deserialize<Dictionary<string, Item>>(fs) ?? new Dictionary<string, Item>();
        }
        foreach (var item in _keyring)
            if (item.Value.ApiBaseUri == null)
                throw new Exception($"ApiBaseUri is not set for {item.Key}");
            else if (item.Value.ApiAuthToken == null)
                throw new Exception($"ApiAuthToken is not set for {item.Key}");
    }

    public bool IsAuthorizedUser(ClaimsPrincipal user, string collection) {
        Item item = _keyring[collection];
        
        if (user.Identity == null || !user.Identity.IsAuthenticated)
            return false;

        if (user.Identity.Name == null)
            return false;

        string username = user.Identity.Name;

        bool UserIsInList(IEnumerable<string>? userList) {
            if (userList == null)
                return false;
            return userList.Any(listedUser => String.Equals(username, listedUser, StringComparison.OrdinalIgnoreCase));
        }

        bool UserIsInRoleList(IEnumerable<string>? roleList) {
            if (roleList == null)
                return false;
            return roleList.Any(listedRole => user.IsInRole(listedRole));
        }

        bool UserIsDenied() {
            return UserIsInList(item.DenyUsers) || UserIsInRoleList(item.DenyGroups);
        }

        bool UserIsAllowed() {
            return UserIsInList(item.AllowUsers) || UserIsInRoleList(item.AllowGroups);
        }

        return !UserIsDenied() && UserIsAllowed();
    }

    public BasePmpApiClient CreateApiClient(string collection) {
        Item item = _keyring[collection];
        
        return new PmpApiClient(new Uri(item.ApiBaseUri), item.ApiAuthToken);
    }

    public BasePmpApiClient GetApiClient(ClaimsPrincipal user, String collection) {
        if (!IsAuthorizedUser(user, collection))
            throw new UnauthorizedAccessException();
        return CreateApiClient(collection);
    }

    public IEnumerable<string> GetCollectionNames() {
        return _keyring.Keys;
    }
}