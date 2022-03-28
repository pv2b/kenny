using PmpApiClient;
using PmpSqlClient;

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
    public List<RoyalJsonObject>? Objects { get; set; }

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
        return o;
    }

    public static RoyalJsonObject CreateFolder(ResourceGroup rg) {
        var o = new RoyalJsonObject();
        o.Type = "Folder";
        o.Name = rg.Name;
        o.Description = rg.Description;
        o.Objects = new List<RoyalJsonObject>();
        return o;
    }

    public static RoyalJsonObject CreateRootObject() {
        var o = new RoyalJsonObject();
        o.Objects = new List<RoyalJsonObject>();
        return o;
    }

    private void SortFolderTree() {
        if (Objects == null) return;
        Objects.Sort((a, b) => (a?.Name ?? string.Empty).CompareTo(b?.Name ?? string.Empty));
        foreach (var obj in Objects) {
            obj.SortFolderTree();
        }
    }

    public static RoyalJsonObject CreateFolderTree(IEnumerable<ResourceGroup> rgs, out Dictionary<long, RoyalJsonObject> foldersByResourceGroupId) {
        var root = CreateRootObject();
        foldersByResourceGroupId = new Dictionary<long, RoyalJsonObject>();
        foreach (var rg in rgs) {
            foldersByResourceGroupId[rg.Id] = CreateFolder(rg);
        }
        foreach (var rg in rgs) {
            var currentFolder = foldersByResourceGroupId[rg.Id];
            var parentFolder = foldersByResourceGroupId.ContainsKey(rg.ParentId) ? foldersByResourceGroupId[rg.ParentId] : root;
            parentFolder.AddChild(currentFolder);
        }

        root.SortFolderTree();
        return root;
    }

    public void AddChild(RoyalJsonObject obj) {
        if (Objects == null) {
            Objects = new List<RoyalJsonObject>();
        }
        Objects.Add(obj);
    }
}