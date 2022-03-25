namespace PmpApiClient;

public class Resource {
    public ResourceSummary Summary { get; }
    public ResourceDetails? Details { get; }
    public IEnumerable<ResourceGroupSummary> Groups { get; }
    public Resource(ResourceSummary summary, ResourceDetails? details, IEnumerable<ResourceGroupSummary> groups) {
        Summary = summary;
        Details = details;
        Groups = groups;
    }
}