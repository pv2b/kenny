using PmpApiClient;
using PmpSqlClient;
public class CrawlerCache {
    private Dictionary<string, Resource> Resources;
    private Dictionary<long, ResourceGroup> ResourceGroups;

    public CrawlerCache() {
        Resources = new Dictionary<string, Resource>();
        ResourceGroups = new Dictionary<long, ResourceGroup>();
    }

    public Resource GetResource(string resourceId) {
        return Resources[resourceId];
    }

    public ResourceGroup GetResourceGroup(long resourceGroupId) {
        return ResourceGroups[resourceGroupId];
    }

    public IEnumerable<Resource> GetResourceList() {
        return Resources.Values;
    }

    public IEnumerable<ResourceGroup> GetResourceGroupList() {
        return ResourceGroups.Values;
    }

    public void UpdateResources(List<Resource> resources) {
        var d = new Dictionary<string, Resource>();
        foreach (var resource in resources) {
            d[resource.Summary.Id] = resource;
        }
        Resources = d;
    }

    public void UpdateResourceGroups(List<ResourceGroup> resourceGroups) {
        var d = new Dictionary<long, ResourceGroup>();
        foreach (var rg in resourceGroups) {
            d[rg.Id] = rg;
        }
        ResourceGroups = d;
    }
}