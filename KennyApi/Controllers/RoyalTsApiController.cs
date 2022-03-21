using Microsoft.AspNetCore.Mvc;
using PmpApiClient;
using System.Text.Json;
using System.IO;

namespace KennyApi.Controllers;

[ApiController]
[Route("")]
public class RoyalTsApiController : ControllerBase
{
    private readonly ILogger<RoyalTsApiController> _logger;

    public RoyalTsApiController(ILogger<RoyalTsApiController> logger)
    {
        _logger = logger;
    }
    
    private static ApiKeyring s_apiKeyring;

    static RoyalTsApiController() {
        var apiKeyringPath = Path.Join(AppContext.BaseDirectory, "ApiKeyring.json");
        s_apiKeyring = new ApiKeyring(apiKeyringPath);
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
        /* skip objects that contain no DNS name because we'll never be able to connect to them or do anything useful with them */
        if (string.IsNullOrWhiteSpace(resource.DnsName))
            return null;
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

    [HttpGet("DynamicFolder")]
    public async Task<Object> GetDynamicFolder(string collection)
    {
        if (!s_apiKeyring.IsAuthorizedUser(HttpContext.User, collection))
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
                var connection = makeRoyalJsonConnectionObject(resource.Details, account);
                if (connection != null ) {
                    objects.Add(connection);
                    objects.Add(makeRoyalJsonCredentialObject(resource.Details, account));
                }
            }
        }
        return new {
            Objects = objects
        };
    }

    [HttpGet("DynamicCredential")]
    public async Task<Object> GetDynamicCredential(string collection, string dynamicCredentialId)
    {
        if (!s_apiKeyring.IsAuthorizedUser(HttpContext.User, collection))
            throw new UnauthorizedAccessException();
        var pmpCredentialId = new PmpCredentialId(dynamicCredentialId);
        var pmpApi = s_apiKeyring.CreateApiClient(collection);
        string reason = $"Requested through kenny by {HttpContext.User.Identity?.Name ?? "unknown user"}";
        var accountPassword = await pmpApi.GetAccountPasswordAsync(pmpCredentialId.ResourceId, pmpCredentialId.AccountId, reason);
        return new {
            Password = accountPassword.Password
        };
    }
}
