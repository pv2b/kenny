using Microsoft.AspNetCore.Mvc;
using RoyalJson;

namespace KennyApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DynamicFolderController : ControllerBase
{
    private readonly ILogger<DynamicFolderController> _logger;

    public DynamicFolderController(ILogger<DynamicFolderController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetDynamicFolder")]
    public RoyalJsonDocument Get(string apiUser)
    {
        throw new NotImplementedException();
    }
}
