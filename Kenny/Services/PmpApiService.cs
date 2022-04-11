using PmpApiClient;
using PmpSqlClient;
using System.Security.Claims;

public class PmpApiService {
    public ApiKeyring ApiKeyring;
    private CrawlerCache _crawlerCache;
    public PmpApiService(IConfiguration configRoot, ApiKeyring apiKeyring, CrawlerCache crawlerCache) {
        ApiKeyring = apiKeyring;
        _crawlerCache = crawlerCache;
    }

    public BasePmpApiClient CreateApiClient(string collection) {
        return ApiKeyring.CreateApiClient(collection);
    }

    public PmpSqlClient.PmpSqlClient CreateSqlClient(string collection) {
        return ApiKeyring.CreateSqlClient(collection);
    }

    public bool IsAuthorizedUser(ClaimsPrincipal user, string collection, Resource resource) {
        foreach (var rgsummary in resource.Groups) {
            var rgs = _crawlerCache.GetResourceGroupDict(collection);
            if (!rgs.ContainsKey(rgsummary.Id))
                continue;
            var rg = rgs[rgsummary.Id];

            foreach (var agrp in rg.AllowGroups) {
                if (user.IsInRole(agrp)) return true;
            }
        }
        return false;
     }
}