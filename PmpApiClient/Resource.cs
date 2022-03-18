namespace PmpApiClient;

public class Resource {
    public ResourceDetails Details { get; }
    public IEnumerable<ResourceGroupSummary> Groups { get; }
    public Resource(ResourceDetails details, IEnumerable<ResourceGroupSummary> groups) {
        Details = details;
        Groups = groups;
    }
}