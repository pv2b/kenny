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
        var acl = ApiKeyring.GetAcl(collection);
        if (acl == null) return false;

        var rgsummary = resource.Groups.FirstOrDefault(g => !g.Name?.Equals("Default Group") ?? true);
        if (rgsummary == null)
            return false;

        var rgs = new Dictionary<long, ResourceGroup>();
        foreach (var rg_ in _crawlerCache.ResourceGroups[collection]) {
            rgs[rg_.Id!] = rg_;
        }
        var rg = rgs[rgsummary.Id];

        foreach (var ace in acl) {
             var action = ace.Check(user, rg, rgs);
             if (action == ResourceGroupAce.AceAction.ALLOW) return true;
             if (action == ResourceGroupAce.AceAction.DENY) return false;
         }
         return false;
     }
}