using System.IO;
using System.Text.Json;
using PmpApiClient;

var apiKeyringPath = Path.Join(AppContext.BaseDirectory, "ApiKeyring.json");
var apiKeyring = new ApiKeyring(apiKeyringPath);

var collectionNames = apiKeyring.GetCollectionNames();

foreach (string collection in collectionNames) {
    Console.WriteLine($"Crawling collection {collection}...");
    var pmpApiClient = apiKeyring.CreateApiClient(collection);
    var resourceFilePath = Path.Join(AppContext.BaseDirectory, $"Resources-{collection}.json");
    var resourceFileTempPath = Path.Join(AppContext.BaseDirectory, $"Resources-{collection}.json.tmp");

    List<Resource> resources = new List<Resource>();
    await foreach (var resource in pmpApiClient.GetAllResourcesAsync()) {
        resources.Add(resource);
    }
    using (FileStream fs = File.Open(resourceFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
        JsonSerializer.Serialize<List<Resource>>(fs, resources);
    }
    File.Move(resourceFileTempPath, resourceFilePath);
}
Console.WriteLine("Done!");