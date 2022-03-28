using PmpApiClient;
using PmpSqlClient;
public class CrawlerCache {
    public Dictionary<string, List<Resource>> Resources;
    public Dictionary<string, List<ResourceGroup>> ResourceGroups;

    public CrawlerCache() {
        Resources = new Dictionary<string, List<Resource>>();
        ResourceGroups = new Dictionary<string, List<ResourceGroup>>();
    }
}