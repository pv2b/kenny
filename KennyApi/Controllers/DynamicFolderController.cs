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

    private Object makeRoyalJsonConnectionObject(ResourceSummary resourceSummary, ResourceDetails resourceDetails, ResourceDetails.Account account) {
        return new {
            Type="TerminalConnection",
            Name=$"{resourceSummary.Name} ({account.Name})",
            ComputerName=resourceDetails.DnsName,
            CustomField1=resourceSummary.Id,
            Path="Connections",
            CredentialID=$"PmpCred_{account.Id}",
        };
    }

    private Object makeRoyalJsonCredentialObject(ResourceSummary resourceSummary, ResourceDetails resourceDetails, ResourceDetails.Account account) {
        return new {
            Type="DynamicCredential",
            Name=$"PMP credential for {resourceSummary.Name} ({account.Name})",
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
        IAsyncEnumerable<(ResourceSummary, ResourceDetails)> resources = pmpApi.GetAllResourceDetailsAsync();
        var objects = new List<Object>();
        await foreach (var (resourceSummary, resourceDetails) in resources) {
            foreach (var account in resourceDetails.Accounts ?? Enumerable.Empty<ResourceDetails.Account>()) {
                objects.Add(makeRoyalJsonConnectionObject(resourceSummary, resourceDetails, account));
                objects.Add(makeRoyalJsonCredentialObject(resourceSummary, resourceDetails, account));
            }
        }
        return new {
            Objects = objects
        };
    }
}
