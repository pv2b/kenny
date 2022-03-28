namespace PmpSqlClient;

public class ResourceGroup {
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ResourceGroup(long id, long parentId, string name, string description) {
        Id = id;
        ParentId = parentId;
        Name = name;
        Description = description;
    }
}
