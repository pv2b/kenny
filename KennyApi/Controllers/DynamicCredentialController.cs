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
    public async Task<Object> Get(string apiUser, string resourceId, string accountId)
    {
        var pmpApi = PmpApiClientStore.GetClient(apiUser);
        string reason = "Requested through kenny";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(resourceId, accountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
