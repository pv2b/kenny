using PmpApiClient;
using PmpSqlClient;
using System.Threading;
using System.Text.Json;

public class PmpCrawlerService : IHostedService, IDisposable
{
    private readonly ILogger<PmpCrawlerService> _logger;
    private readonly ApiKeyring _apiKeyring;
    private Timer _timer = null!;
    private uint _crawlRunning = 0;
    private CrawlerCache _cache;

    public PmpCrawlerService(ILogger<PmpCrawlerService> logger, ApiKeyring apiKeyring, CrawlerCache cache)
    {
        _logger = logger;
        _apiKeyring = apiKeyring;
        _cache = cache;
    }

    private string GetCollectionFilename(string prefix, string collection) {
        return Path.Join(AppContext.BaseDirectory, $"{prefix}-{collection}.json");
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        var collectionNames = _apiKeyring.GetCollectionNames();

        foreach (string collection in collectionNames) {
            string resourcesFile = GetCollectionFilename("Resources", collection);
            try {
                using (FileStream fs = System.IO.File.Open(resourcesFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                    var resources = await JsonSerializer.DeserializeAsync<List<Resource>>(fs);
                    if (resources != null )
                        _cache.UpdateResources(collection, resources);
                }
            } catch (FileNotFoundException) {
                // If we can't open the cache file, no big deal, that just means the data will be crawled soon... */
            }

            string resourceGroupsFile = GetCollectionFilename("ResourceGroups", collection);
            try {
                using (FileStream fs = System.IO.File.Open(resourceGroupsFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                    var rgs = await JsonSerializer.DeserializeAsync<List<ResourceGroup>>(fs);
                    if (rgs != null )
                        _cache.UpdateResourceGroups(collection, rgs);
                }
            } catch (FileNotFoundException) {
                // If we can't open the cache file, no big deal, that just means the data will be crawled soon... */
            }
        }

        _timer = new System.Threading.Timer(DoWork, stoppingToken, TimeSpan.Zero, 
            TimeSpan.FromSeconds(300));
    }

    private async Task CrawlApi() {
        var collectionNames = _apiKeyring.GetCollectionNames();

        foreach (string collection in collectionNames) {
            Console.WriteLine($"Crawling collection {collection} API...");
            try {
                var pmpApiClient = _apiKeyring.CreateApiClient(collection);
                var resourceFilePath = GetCollectionFilename("Resources", collection);
                var resourceFileTempPath = $"{resourceFilePath}.tmp";

                List<Resource> resources = new List<Resource>();
                await foreach (var resource in pmpApiClient.GetAllResourcesAsync()) {
                    resources.Add(resource);
                }
                _cache.UpdateResources(collection, resources);
                using (FileStream fs = File.Open(resourceFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    JsonSerializer.Serialize<List<Resource>>(fs, resources, new JsonSerializerOptions { WriteIndented = true });
                }
                File.Move(resourceFileTempPath, resourceFilePath, true);
            } catch (Exception e) {
                _logger.LogError(e, $"Error crawling collection {collection} API");
            }
        }
    }

    private async Task CrawlSql() {
        var collectionNames = _apiKeyring.GetCollectionNames();

        foreach (string collection in collectionNames) {
            Console.WriteLine($"Crawling collection {collection} SQL...");
            try {
                var pmpSqlClient = _apiKeyring.CreateSqlClient(collection);
                var rgFilePath = GetCollectionFilename("ResourceGroups", collection);
                var rgFileTempPath = $"{rgFilePath}.tmp";

                var rgs = new List<ResourceGroup>();
                await foreach (var rg in pmpSqlClient.GetResourceGroupsAsync()) {
                    rgs.Add(rg);
                }
                _cache.UpdateResourceGroups(collection, rgs);
                using (FileStream fs = File.Open(rgFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    JsonSerializer.Serialize<List<ResourceGroup>>(fs, rgs, new JsonSerializerOptions { WriteIndented = true });
                }
                File.Move(rgFileTempPath, rgFilePath, true);
            } catch (Exception e) {
                _logger.LogError(e, $"Error crawling collection {collection} SQL");
            }
        }
    }

    private async void DoWork(object? stoppingToken_)
    {
        CancellationToken stoppingToken = (CancellationToken?)stoppingToken_ ?? default(CancellationToken);
        uint crawlAlreadyRunning = Interlocked.Exchange(ref _crawlRunning, 1);
        if (crawlAlreadyRunning == 1) {
            _logger.LogInformation("Time to run Pmp Crawler, but crawler is already running... skipping!");
            // crawl was already running, so don't start it again...
            return;
        }

        try {
            _logger.LogInformation("Pmp Crawler started");
            var apiTask = CrawlApi();
            var sqlTask = CrawlSql();
            await apiTask;
            await sqlTask;
        } catch (Exception e) {
            _logger.LogError(e, "Error crawling");
        } finally {
            _crawlRunning = 0;
            _logger.LogInformation("Pmp Crawler finished");
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}