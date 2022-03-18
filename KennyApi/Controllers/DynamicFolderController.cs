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

    public class RoyalJsonObject {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? ComputerName { get; set; }
        public string? Description { get; set; }
        public string? Path { get; set; }
        public string? CredentialId { get; set; }
        public string? TerminalConnectionType { get; set; }
    }

    private RoyalJsonObject? makeRoyalJsonConnectionObject(ResourceDetails resource, ResourceDetails.Account account) {
        var o = new RoyalJsonObject();
        switch(resource.Type) {
            case "Windows":
                o.Type = "RemoteDesktopConnection";
                break;

            case "Linux":
                o.Type = "TerminalConnection";
                o.TerminalConnectionType = "SSH";
                break;

            default:
                return null;
        }

        o.Name = $"{resource.Name} ({account.Name})";
        o.ComputerName = resource.DnsName;
        o.Path = "Connections";
        o.CredentialId = new PmpCredentialId(resource.Id, account.Id).ToString();
        o.Description = resource.Description;

        return o;
    }

    private Object makeRoyalJsonCredentialObject(ResourceDetails resource, ResourceDetails.Account account) {
        return new {
            Type="DynamicCredential",
            Name=$"PMP credential for {resource.Name} ({account.Name})",
            Id=new PmpCredentialId(resource.Id, account.Id).ToString(),
            Username=account.Name,
            Path="Credentials",
        };
    }

    [HttpGet(Name = "GetDynamicFolder")]
    public async Task<Object> Get(string collection)
    {
        if (!Globals.ApiKeyring.IsAuthorizedUser(HttpContext.User, collection))
            throw new UnauthorizedAccessException();
        string filename = Path.Join(AppContext.BaseDirectory, $"Resources-{collection}.json");
        IEnumerable<ResourceDetails>? resources;
        using (FileStream fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
            resources = await JsonSerializer.DeserializeAsync<List<ResourceDetails>>(fs);
        }
        if (resources == null) {
            throw new Exception("Resources is null!");
        }
        var objects = new List<Object>();
        foreach (var resource in resources) {
            foreach (var account in resource.Accounts ?? Enumerable.Empty<ResourceDetails.Account>()) {
                /* skip objects that contain no DNS name because we'll never be able to connect to them or do anything useful with them */
                if (string.IsNullOrWhiteSpace(resource.DnsName))
                    continue;
                var connection = makeRoyalJsonConnectionObject(resource, account);
                if (connection != null ) {
                    objects.Add(connection);
                    objects.Add(makeRoyalJsonCredentialObject(resource, account));
                }
            }
        }
        return new {
            Objects = objects
        };
    }
}
