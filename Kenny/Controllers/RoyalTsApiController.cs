using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
using System.Text.Json;
using System.IO;

namespace Kenny.Controllers;

[ApiController]
[Route("")]
public class RoyalTsApiController : ControllerBase
{
    private readonly ILogger<RoyalTsApiController> _logger;
    private readonly PmpApiService _pmpApiService;
    private readonly CrawlerCache _crawlerCache;

    public RoyalTsApiController(ILogger<RoyalTsApiController> logger, PmpApiService pmpApiService, CrawlerCache crawlerCache)
    {
        _logger = logger;
        _pmpApiService = pmpApiService;
        _crawlerCache = crawlerCache;
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
            if (!_pmpApiService.IsAuthorizedUser(HttpContext.User, collection, resource))
                continue;
            if (resource.Details?.Accounts == null)
                continue;
            foreach (var account in resource.Details.Accounts) {
                var credential = RoyalJsonObject.CreateDynamicCredential(resource.Details, account);
                var group = resource.Groups.FirstOrDefault(g => !g.Name?.Equals("Default Group") ?? true);
                var folder = (group != null) ? connectionFolders[group.Id] : root;
                if (credential != null) folder.AddChild(credential);
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
        if (!_pmpApiService.IsAuthorizedUser(HttpContext.User, collection, resource))
            throw new UnauthorizedAccessException();
        var pmpApi = _pmpApiService.CreateApiClient(collection);
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(pmpCredentialId.ResourceId, pmpCredentialId.AccountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
