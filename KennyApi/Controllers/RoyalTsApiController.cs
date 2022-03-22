using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
using System.Text.Json;
using System.IO;

namespace KennyApi.Controllers;

[ApiController]
[Route("")]
public class RoyalTsApiController : ControllerBase
{
    private readonly ILogger<RoyalTsApiController> _logger;
    private readonly PmpApiService _pmpApiService;

    public RoyalTsApiController(ILogger<RoyalTsApiController> logger, PmpApiService pmpApiService)
    {
        _logger = logger;
        _pmpApiService = pmpApiService;
    }
    
    [HttpGet("DynamicFolder")]
    public async Task<Object> GetDynamicFolder(string collection)
    {
        if (!_pmpApiService.IsAuthorizedUser(HttpContext.User, collection))
            throw new UnauthorizedAccessException();
        string filename = Path.Join(AppContext.BaseDirectory, $"Resources-{collection}.json");
        IEnumerable<Resource>? resources;
        using (FileStream fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
            resources = await JsonSerializer.DeserializeAsync<List<Resource>>(fs);
        }
        if (resources == null) {
            throw new Exception("Resources is null!");
        }
        var objects = new List<Object>();
        foreach (var resource in resources) {
            foreach (var account in resource.Details.Accounts ?? Enumerable.Empty<ResourceDetails.Account>()) {
                var connection = RoyalJsonObject.CreateConnection(resource.Details, account);
                if (connection != null ) {
                    objects.Add(connection);
                    objects.Add(RoyalJsonObject.CreateDynamicCredential(resource.Details, account));
                }
            }
        }
        return new {
            Objects = objects
        };
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
