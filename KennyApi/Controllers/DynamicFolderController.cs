using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
using System.Text.Json;
using System.IO;

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
                objects.Add(makeRoyalJsonConnectionObject(resource, account));
                objects.Add(makeRoyalJsonCredentialObject(resource, account));
            }
        }
        return new {
            Objects = objects
        };
    }
}
