using PmpApiClient;
using System.Threading;
using System.Text.Json;

public class PmpCrawlerService : IHostedService, IDisposable
{
    private readonly ILogger<PmpCrawlerService> _logger;
    private readonly PmpApiService _pmpApiService;
    private Timer _timer = null!;
    private uint _crawlRunning = 0;
    private CrawlerCache _cache;

    public PmpCrawlerService(ILogger<PmpCrawlerService> logger, PmpApiService pmpApiService, CrawlerCache cache)
    {
        _logger = logger;
        _pmpApiService = pmpApiService;
        _cache = cache;
    }

    private string GetCollectionFilename(string collection) {
        return Path.Join(AppContext.BaseDirectory, $"Resources-{collection}.json");
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        var collectionNames = _pmpApiService.ApiKeyring.GetCollectionNames();

        foreach (string collection in collectionNames) {
            string filename = GetCollectionFilename(collection);
            try {
                using (FileStream fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                    var resources = await JsonSerializer.DeserializeAsync<List<Resource>>(fs);
                    if (resources != null )
                        _cache.Resources[collection] = resources;
                }
            } catch (FileNotFoundException) {
                // If we can't open the cache file, no big deal, that just means the data will be crawled soon... */
            }
        }

        _timer = new System.Threading.Timer(DoWork, stoppingToken, TimeSpan.Zero, 
            TimeSpan.FromSeconds(300));
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

        _logger.LogInformation("Pmp Crawler started");
        var collectionNames = _pmpApiService.ApiKeyring.GetCollectionNames();

        foreach (string collection in collectionNames) {
            Console.WriteLine($"Crawling collection {collection}...");
            var pmpApiClient = _pmpApiService.CreateApiClient(collection);
            var resourceFilePath = GetCollectionFilename(collection);
            var resourceFileTempPath = $"{resourceFilePath}.tmp";

            List<Resource> resources = new List<Resource>();
            await foreach (var resource in pmpApiClient.GetAllResourcesAsync()) {
                resources.Add(resource);
            }
            _cache.Resources[collection] = resources;
            using (FileStream fs = File.Open(resourceFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                JsonSerializer.Serialize<List<Resource>>(fs, resources, new JsonSerializerOptions { WriteIndented = true });
            }
            File.Move(resourceFileTempPath, resourceFilePath, true);
        }
        _logger.LogInformation("Pmp Crawler finished");
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