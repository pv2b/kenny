using System.IO;
using System.Text.Json;
using PmpApiClient;

var apiKeyringPath = "ApiKeyring.json";
var apiKeyring = new ApiKeyring(apiKeyringPath);

var collectionNames = apiKeyring.GetCollectionNames();

foreach (string collection in collectionNames) {
    Console.WriteLine($"Crawling collection {collection}...");
    var pmpApiClient = apiKeyring.CreateApiClient(collection);
    var resourceFilePath = $"Resources-{collection}.json";
    var resourceFileTempPath = $"Resources-{collection}.json.tmp";

    var resources = new List<ResourceDetails>();
    await foreach (var resource in pmpApiClient.GetAllResourceDetailsAsync()) {
        resources.Add(resource);
    }
    using (FileStream fs = File.Open(resourceFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
        JsonSerializer.Serialize<List<ResourceDetails>>(fs, resources);
    }
    File.Move(resourceFileTempPath, resourceFilePath, true);
}
Console.WriteLine("Done!");