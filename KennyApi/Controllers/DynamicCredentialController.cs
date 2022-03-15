using Microsoft.AspNetCore.Mvc;
using PmpApiClient;

namespace KennyApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DynamicCredentialController : ControllerBase
{
    private readonly ILogger<DynamicCredentialController> _logger;

    public DynamicCredentialController(ILogger<DynamicCredentialController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetDynamicCredential")]
    public async Task<Object> Get(string collection, string resourceId, string accountId)
    {
        var pmpApi = Globals.ApiKeyring.GetApiClient(HttpContext.User, collection);
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(resourceId, accountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
