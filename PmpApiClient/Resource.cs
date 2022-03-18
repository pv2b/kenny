namespace PmpApiClient;

public class Resource {
    public ResourceDetails Details { get; }
    public Resource(ResourceDetails details) {
        Details = details;
    }
}