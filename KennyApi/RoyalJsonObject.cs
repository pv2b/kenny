using PmpApiClient;

public class RoyalJsonObject {
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? ComputerName { get; set; }
    public string? Description { get; set; }
    public string? Path { get; set; }
    public string? CredentialId { get; set; }
    public string? TerminalConnectionType { get; set; }
    public string? Id { get; set; }
    public string? Username { get; set; }

    public static RoyalJsonObject? CreateConnection(ResourceDetails resource, ResourceDetails.Account account) {
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

    public static RoyalJsonObject CreateDynamicCredential(ResourceDetails resource, ResourceDetails.Account account) {
        var o = new RoyalJsonObject();
        o.Type="DynamicCredential";
        o.Name=$"PMP credential for {resource.Name} ({account.Name})";
        o.Id=new PmpCredentialId(resource.Id, account.Id).ToString();
        o.Username=account.Name;
        o.Path="Credentials";
        return o;
    }
}