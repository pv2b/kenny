namespace PmpApiClient;

public class Resource {
    public ResourceSummary Summary { get; }
    public ResourceDetails Details { get; }
    public Resource(ResourceSummary summary, ResourceDetails details) {
        Summary = summary;
        Details = details;
    }
}