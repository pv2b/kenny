using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
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

    public RoyalTsApiController(ILogger<RoyalTsApiController> logger, CrawlerCache crawlerCache, PmpApiFactory apiKeyring)
    {
        _logger = logger;
        _crawlerCache = crawlerCache;
        _apiKeyring = apiKeyring;
    }

    public bool IsAuthorizedUser(Resource resource) {
        foreach (var rgsummary in resource.Groups) {
            var rgs = _crawlerCache.GetResourceGroupDict();
            if (!rgs.ContainsKey(rgsummary.Id))
                continue;
            var rg = rgs[rgsummary.Id];

            foreach (var agrp in rg.AllowGroups) {
                if (HttpContext.User.IsInRole(agrp)) return true;
            }
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
            if (!IsAuthorizedUser(resource))
                continue;
            if (resource.Details?.Accounts == null)
                continue;
            foreach (var account in resource.Details.Accounts) {
                foreach (var group in resource.Groups) {
                    if (group != null && connectionFolders.ContainsKey(group.Id)) {
                        var credential = RoyalJsonObject.CreateDynamicCredential(group, resource.Details, account);
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
    public async Task<Object> GetDynamicCredential(string dynamicCredentialId)
    {
        var pmpCredentialId = new PmpCredentialId(dynamicCredentialId);
        var resource = _crawlerCache.GetResource(pmpCredentialId.ResourceId);
        if (!IsAuthorizedUser(resource))
            throw new UnauthorizedAccessException();
        var pmpApi = _apiKeyring.CreateApiClient();
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(pmpCredentialId.ResourceId, pmpCredentialId.AccountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
