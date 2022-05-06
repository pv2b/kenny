using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
using PmpSqlClient;
using System.Text.Json;
using System.IO;

namespace Kenny.Controllers;

[ApiController]
[Route("")]
public class RoyalTsApiController : ControllerBase
{
    private PmpApiFactory _apiKeyring;
    private readonly ILogger<RoyalTsApiController> _logger;
    private readonly CrawlerCache _crawlerCache;
    private readonly Guid _royalTSNamespaceGuid;

    public RoyalTsApiController(ILogger<RoyalTsApiController> logger, CrawlerCache crawlerCache, PmpApiFactory apiKeyring, IConfiguration config)
    {
        _logger = logger;
        _crawlerCache = crawlerCache;
        _apiKeyring = apiKeyring;
        _royalTSNamespaceGuid = config.GetValue<Guid>("RoyalTSNamespaceGuid");
    }

    public bool IsAuthorizedUser(Resource resource) {
        foreach (var rgsummary in resource.Groups) {
            if (IsAuthorizedUser(rgsummary)) return true;
        }
        return false;
    }

    public bool IsAuthorizedUser(ResourceGroupSummary rgs) {
        ResourceGroup rg;
        try {
            rg = _crawlerCache.GetResourceGroup(rgs.Id);
        } catch (KeyNotFoundException) {
            return false;
        }
        return IsAuthorizedUser(rg);
    }

    public bool IsAuthorizedUser(ResourceGroup rg) {
        foreach (var agrp in rg.AllowGroups) {
            if (HttpContext.User.IsInRole(agrp)) return true;
        }
        return false;
    }
    
    [HttpGet("DynamicFolder")]
    public Object GetDynamicFolder()
    {
        IEnumerable<Resource>? resources = _crawlerCache.GetResourceList();
        if (resources == null) {
            throw new Exception("Resources is null!");
        }
        Dictionary<long, RoyalJsonObject> connectionFolders;
        var root = RoyalJsonObject.CreateFolderTree(_crawlerCache.GetResourceGroupList(), out connectionFolders);

        foreach (var resource in resources) {
            if (resource.Details?.Accounts == null)
                continue;
            foreach (var account in resource.Details.Accounts) {
                foreach (var group in resource.Groups) {
                    if (group != null && IsAuthorizedUser(group) && connectionFolders.ContainsKey(group.Id)) {
                        var credential = RoyalJsonObject.CreateDynamicCredential(_royalTSNamespaceGuid, group, resource.Details, account);
                        connectionFolders[group.Id].AddChild(credential);
                    }
                }
            }
        }
        root.PurgeEmptyFoldersRecursive();
        root.SortFolderRecursive();
        return root;
    }

    [HttpGet("DynamicCredential")]
    public async Task<Object> GetDynamicCredential(string resourceId, string accountId)
    {
        var resource = _crawlerCache.GetResource(resourceId);
        if (!IsAuthorizedUser(resource))
            throw new UnauthorizedAccessException();
        var pmpApi = _apiKeyring.CreateApiClient();
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(resourceId, accountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
