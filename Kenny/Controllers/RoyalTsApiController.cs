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
        if (!_pmpApiService.IsAuthorizedUser(HttpContext.User, collection))
            throw new UnauthorizedAccessException();
        string filename = Path.Join(AppContext.BaseDirectory, $"Resources-{collection}.json");
        IEnumerable<Resource>? resources = _crawlerCache.Resources[collection];
        if (resources == null) {
            throw new Exception("Resources is null!");
        }
        Dictionary<long, RoyalJsonObject> connectionFolders;
        RoyalJsonObject credentialFolder;
        var root = RoyalJsonObject.CreateFolderTree(_crawlerCache.ResourceGroups[collection], out connectionFolders, out credentialFolder);

        foreach (var resource in resources) {
            if (resource.Details?.Accounts == null)
                continue;
            foreach (var account in resource.Details.Accounts) {
                var connection = RoyalJsonObject.CreateConnection(resource.Details, account);
                if (connection != null) {
                    foreach (var group in resource.Groups) {
                        var folder = connectionFolders[group.Id];
                        folder.AddChild(connection);
                    }
                    credentialFolder.AddChild(RoyalJsonObject.CreateDynamicCredential(resource.Details, account));
                }
            }
        }
        return root;
    }

    [HttpGet("DynamicCredential")]
    public async Task<Object> GetDynamicCredential(string collection, string dynamicCredentialId)
    {
        if (!_pmpApiService.IsAuthorizedUser(HttpContext.User, collection))
            throw new UnauthorizedAccessException();
        var pmpCredentialId = new PmpCredentialId(dynamicCredentialId);
        var pmpApi = _pmpApiService.CreateApiClient(collection);
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(pmpCredentialId.ResourceId, pmpCredentialId.AccountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
