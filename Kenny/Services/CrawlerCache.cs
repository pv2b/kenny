using PmpApiClient;
using PmpSqlClient;
public class CrawlerCache {
    public Dictionary<string, Resource> ResourcesDict;
    public Dictionary<long, ResourceGroup> ResourceGroupsDict;

    public CrawlerCache() {
        ResourcesDict = new Dictionary<string, Resource>();
        ResourceGroupsDict = new Dictionary<long, ResourceGroup>();
    }

    public Resource GetResource(string resourceId) {
        return ResourcesDict[resourceId];
    }

    public IEnumerable<Resource> GetResourceList() {
        return ResourcesDict.Values;
    }

    public IEnumerable<ResourceGroup> GetResourceGroupList() {
        return ResourceGroupsDict.Values;
    }
    
    public IDictionary<long, ResourceGroup> GetResourceGroupDict() {
        return ResourceGroupsDict;
    }    

    public void UpdateResources(List<Resource> resources) {
        var d = new Dictionary<string, Resource>();
        foreach (var resource in resources) {
            d[resource.Summary.Id] = resource;
        }
        ResourcesDict = d;
    }

    public void UpdateResourceGroups(List<ResourceGroup> resourceGroups) {
        var d = new Dictionary<long, ResourceGroup>();
        foreach (var rg in resourceGroups) {
            d[rg.Id] = rg;
        }
        ResourceGroupsDict = d;
    }
}