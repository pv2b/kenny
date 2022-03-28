using PmpApiClient;
using PmpSqlClient;
public class CrawlerCache {
    private class Item {
        public Dictionary<string, Resource> ResourcesDict = new Dictionary<string, Resource>();
        public Dictionary<long, ResourceGroup> ResourceGroupsDict = new Dictionary<long, ResourceGroup>();
    }
    private Dictionary<string, Item> _collectionCache;

    private Item GetOrCreateItem(string collection) {
        if (!_collectionCache.ContainsKey(collection)) {
            _collectionCache[collection] = new Item();
        }
        return _collectionCache[collection];
    }

    public CrawlerCache() {
        _collectionCache = new Dictionary<string, Item>();
    }

    public Resource GetResource(string collection, string resourceId) {
        var item = GetOrCreateItem(collection);
        return item.ResourcesDict[resourceId];
    }

    public IEnumerable<Resource> GetResourceList(string collection) {
        var item = GetOrCreateItem(collection);
        return item.ResourcesDict.Values;
    }

    public IEnumerable<ResourceGroup> GetResourceGroupList(string collection) {
        var item = GetOrCreateItem(collection);
        return item.ResourceGroupsDict.Values;
    }
    
    public IDictionary<long, ResourceGroup> GetResourceGroupDict(string collection) {
        var item = GetOrCreateItem(collection);
        return item.ResourceGroupsDict;
    }    

    public void UpdateResources(string collection, List<Resource> resources) {
        var item = GetOrCreateItem(collection);
        var d = new Dictionary<string, Resource>();
        foreach (var resource in resources) {
            d[resource.Summary.Id] = resource;
        }
        item.ResourcesDict = d;
    }

    public void UpdateResourceGroups(string collection, List<ResourceGroup> resourceGroups) {
        var item = GetOrCreateItem(collection);
        var d = new Dictionary<long, ResourceGroup>();
        foreach (var rg in resourceGroups) {
            d[rg.Id] = rg;
        }
        item.ResourceGroupsDict = d;
    }
}