using Microsoft.AspNetCore.Mvc;
using RoyalJson;

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
    public Task<RoyalJsonDynamicCredential> Get(string apiUser, string credentialId)
    {
        var pmpApi = PmpApiClientStore.GetClient(apiUser);
        throw new NotImplementedException();
    }
}
