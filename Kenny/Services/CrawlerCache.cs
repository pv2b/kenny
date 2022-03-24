using PmpApiClient;
public class CrawlerCache {
    public Dictionary<string, List<Resource>> Resources;

    public CrawlerCache() {
        Resources = new Dictionary<string, List<Resource>>();
    }
}