using Microsoft.AspNetCore.Mvc;
using PmpApiClient;

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

    private Object makeRoyalJsonConnectionObject(Resource resource, ResourceAccountList resourceDetails, ResourceAccountList.Account account) {
        return new {
            Type="TerminalConnection",
            Name=$"{resource.Name} ({account.Name})",
            ComputerName=resourceDetails.DnsName,
            CustomField1=resource.Id,
            Path="Connections",
            CredentialID=$"PmpCred_{account.Id}",
        };
    }

    private Object makeRoyalJsonCredentialObject(Resource resource, ResourceAccountList resourceDetails, ResourceAccountList.Account account) {
        return new {
            Type="DynamicCredential",
            Name=$"PMP credential for {resource.Name} ({account.Name})",
            Id=$"PmpCred_{account.Id}",
            Username=account.Name,
            Path="Credentials",
            TerminalConnectionType="SSH"
        };
    }

    [HttpGet(Name = "GetDynamicFolder")]
    public async Task<Object> Get(string collection)
    {
        if (!Globals.ApiKeyring.IsAuthorizedUser(HttpContext.User, collection))
            throw new UnauthorizedAccessException();
        var pmpApi = Globals.ApiKeyring.CreateApiClient(collection);
        IAsyncEnumerable<(Resource, ResourceAccountList)> resources = pmpApi.GetAllResourceAccountListAsync();
        var objects = new List<Object>();
        await foreach (var (resource, resourceDetails) in resources) {
            foreach (var account in resourceDetails.Accounts ?? Enumerable.Empty<ResourceAccountList.Account>()) {
                objects.Add(makeRoyalJsonConnectionObject(resource, resourceDetails, account));
                objects.Add(makeRoyalJsonCredentialObject(resource, resourceDetails, account));
            }
        }
        return new {
            Objects = objects
        };
    }
}
