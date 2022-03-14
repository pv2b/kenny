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
    public async Task<Object> Get(string apiUser)
    {
        var pmpApi = PmpApiClientStore.GetClient(apiUser);
        IEnumerable<(Resource, Task<ResourceAccountList>)> resources = await pmpApi.GetAllResourceAccountListAsync();
        var objects = new List<Object>();
        foreach (var (resource, resourceAccountListTask) in resources) {
            var resourceDetails = await resourceAccountListTask;
            foreach (var account in resourceDetails.Accounts) {
                objects.Add(makeRoyalJsonConnectionObject(resource, resourceDetails, account));
                objects.Add(makeRoyalJsonCredentialObject(resource, resourceDetails, account));
            }
        }
        return new {
            Objects = objects
        };
    }
}
