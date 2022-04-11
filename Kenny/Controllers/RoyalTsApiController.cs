using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
using System.Text.Json;
using System.IO;

namespace Kenny.Controllers;

[ApiController]
[Route("")]
public class RoyalTsApiController : ControllerBase
{
    private ApiKeyring _apiKeyring;
    private readonly ILogger<RoyalTsApiController> _logger;
    private readonly CrawlerCache _crawlerCache;

    public RoyalTsApiController(ILogger<RoyalTsApiController> logger, CrawlerCache crawlerCache, ApiKeyring apiKeyring)
    {
        _logger = logger;
        _crawlerCache = crawlerCache;
        _apiKeyring = apiKeyring;
    }

    public bool IsAuthorizedUser(string collection, Resource resource) {
        foreach (var rgsummary in resource.Groups) {
            var rgs = _crawlerCache.GetResourceGroupDict(collection);
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
    public Object GetDynamicFolder(string collection)
    {
        IEnumerable<Resource>? resources = _crawlerCache.GetResourceList(collection);
        if (resources == null) {
            throw new Exception("Resources is null!");
        }
        Dictionary<long, RoyalJsonObject> connectionFolders;
        var root = RoyalJsonObject.CreateFolderTree(_crawlerCache.GetResourceGroupList(collection), out connectionFolders);

        foreach (var resource in resources) {
            if (!IsAuthorizedUser(collection, resource))
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
    public async Task<Object> GetDynamicCredential(string collection, string dynamicCredentialId)
    {
        var pmpCredentialId = new PmpCredentialId(dynamicCredentialId);
        var resource = _crawlerCache.GetResource(collection, pmpCredentialId.ResourceId);
        if (!IsAuthorizedUser(collection, resource))
            throw new UnauthorizedAccessException();
        var pmpApi = _apiKeyring.CreateApiClient(collection);
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(pmpCredentialId.ResourceId, pmpCredentialId.AccountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
