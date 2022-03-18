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

    private Object makeRoyalJsonConnectionObject(Resource resource, Resource.Account account) {
        return new {
            Type="TerminalConnection",
            Name=$"{resource.Name} ({account.Name})",
            ComputerName=resource.DnsName,
            CustomField1=resource.Id,
            Path="Connections",
            CredentialID=$"PmpCred_{account.Id}",
            Description=resource.Description
        };
    }

    private Object makeRoyalJsonCredentialObject(Resource resource, Resource.Account account) {
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
            foreach (var account in resource.Accounts ?? Enumerable.Empty<Resource.Account>()) {
                objects.Add(makeRoyalJsonConnectionObject(resource, account));
                objects.Add(makeRoyalJsonCredentialObject(resource, account));
            }
        }
        return new {
            Objects = objects
        };
    }
}
