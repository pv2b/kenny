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

    private Object makeRoyalJsonConnectionObject(Resource resource, ResourceDetails.Account account) {
        return new {
            Type="TerminalConnection",
            Name=$"{resource.Summary.Name} ({account.Name})",
            ComputerName=resource.Details.DnsName,
            CustomField1=resource.Summary.Id,
            Path="Connections",
            CredentialID=$"PmpCred_{account.Id}",
        };
    }

    private Object makeRoyalJsonCredentialObject(Resource resource, ResourceDetails.Account account) {
        return new {
            Type="DynamicCredential",
            Name=$"PMP credential for {resource.Summary.Name} ({account.Name})",
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
        IAsyncEnumerable<Resource> resources = pmpApi.GetAllResourcesAsync();
        var objects = new List<Object>();
        await foreach (var resource in resources) {
            foreach (var account in resource.Details.Accounts ?? Enumerable.Empty<ResourceDetails.Account>()) {
                objects.Add(makeRoyalJsonConnectionObject(resource, account));
                objects.Add(makeRoyalJsonCredentialObject(resource, account));
            }
        }
        return new {
            Objects = objects
        };
    }
}
