using PmpApiClient;
using System.Security.Claims;

public class PmpApiService {
    public ApiKeyring ApiKeyring;
    public PmpApiService() {
        var apiKeyringPath = Path.Join(AppContext.BaseDirectory, "ApiKeyring.json");
        ApiKeyring = new ApiKeyring(apiKeyringPath); 
    }

    public bool IsAuthorizedUser(ClaimsPrincipal user, string collection) {
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
            return UserIsInList(ApiKeyring.GetDenyUsers(collection)) || UserIsInRoleList(ApiKeyring.GetDenyGroups(collection));
        }

        bool UserIsAllowed() {
            return UserIsInList(ApiKeyring.GetAllowUsers(collection)) || UserIsInRoleList(ApiKeyring.GetAllowGroups(collection));
        }

        return !UserIsDenied() && UserIsAllowed();
    }

    public BasePmpApiClient CreateApiClient(string collection) {
        return ApiKeyring.CreateApiClient(collection);
    }
}