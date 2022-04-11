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

    public static RoyalJsonObject CreateDynamicCredential(ResourceDetails resource, ResourceDetails.Account account) {
        var o = new RoyalJsonObject();
        o.Type="DynamicCredential";
        o.Name=$"Cred {resource.Name} ({account.Name})";
        o.Id=new PmpCredentialId(resource.Id, account.Id).ToString();
        o.Username=account.Name;
        return o;
    }

    private static RoyalJsonObject CreateFolder(string? name = null, string? description = null) {
        var o = new RoyalJsonObject();
        o.Type = "Folder";
        o.Name = name;
        o.Description = description;
        o.Objects = new List<RoyalJsonObject>();
        return o;
    }

    public static RoyalJsonObject CreateFolder(ResourceGroup rg) {
        return CreateFolder(rg.Name, rg.Description);
    }

    public void SortFolderRecursive() {
        if (Objects == null) return;
        Objects.Sort((a, b) => (a?.Name ?? string.Empty).CompareTo(b?.Name ?? string.Empty));
        foreach (var obj in Objects) {
            obj.SortFolderRecursive();
        }
    }

    public static RoyalJsonObject CreateFolderTree(IEnumerable<ResourceGroup> rgs, out Dictionary<long, RoyalJsonObject> connectionFolders) {
        var root = CreateFolder();
        connectionFolders = new Dictionary<long, RoyalJsonObject>();
        foreach (var rg in rgs) {
            connectionFolders[rg.Id] = CreateFolder(rg);
        }
        foreach (var rg in rgs) {
            var currentFolder = connectionFolders[rg.Id];
            var parentFolder = connectionFolders.ContainsKey(rg.ParentId) ? connectionFolders[rg.ParentId] : root;
            parentFolder.AddChild(currentFolder);
        }

        return root;
    }

    public void AddChild(RoyalJsonObject obj) {
        if (Objects == null) {
            Objects = new List<RoyalJsonObject>();
        }
        Objects.Add(obj);
    }

    public void PurgeEmptyFoldersRecursive() {
        if (Objects == null) return;
        var keptObjects = new List<RoyalJsonObject>();
        foreach (var child in Objects) {
            bool keep = true;
            if (child.Type != null && child.Type.Equals("Folder")) {
                child.PurgeEmptyFoldersRecursive();
                keep = child.Objects != null && child.Objects.Count > 0;
            }
            if (keep) {
                keptObjects.Add(child);
            }
        }
        Objects = keptObjects;
    }
}