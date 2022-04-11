using PmpApiClient;
using PmpSqlClient;
using System.Threading;
using System.Text.Json;

public class PmpCrawlerService : IHostedService, IDisposable
{
    private readonly ILogger<PmpCrawlerService> _logger;
    private readonly PmpApiFactory _pmpApiFactory;
    private Timer _timer = null!;
    private uint _crawlRunning = 0;
    private CrawlerCache _cache;
    private readonly IConfiguration _config;

    public PmpCrawlerService(ILogger<PmpCrawlerService> logger, PmpApiFactory pmpApiFactory, CrawlerCache cache, IConfiguration config)
    {
        _logger = logger;
        _pmpApiFactory = pmpApiFactory;
        _cache = cache;
        _config = config;
    }

    private string GetCacheFilename(string prefix) {
        return Path.Join(AppContext.BaseDirectory, $"{prefix}.json");
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        string resourcesFile = GetCacheFilename("Resources");
        try {
            using (FileStream fs = System.IO.File.Open(resourcesFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                var resources = await JsonSerializer.DeserializeAsync<List<Resource>>(fs);
                if (resources != null )
                    _cache.UpdateResources(resources);
            }
        } catch (FileNotFoundException) {
            // If we can't open the cache file, no big deal, that just means the data will be crawled soon... */
        }

        string resourceGroupsFile = GetCacheFilename("ResourceGroups");
        try {
            using (FileStream fs = System.IO.File.Open(resourceGroupsFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                var rgs = await JsonSerializer.DeserializeAsync<List<ResourceGroup>>(fs);
                if (rgs != null )
                    _cache.UpdateResourceGroups(rgs);
            }
        } catch (FileNotFoundException) {
            // If we can't open the cache file, no big deal, that just means the data will be crawled soon... */
        }

        _timer = new System.Threading.Timer(DoWork, stoppingToken, TimeSpan.Zero, 
            TimeSpan.FromSeconds(300));
    }

    private async Task CrawlApi() {
        Console.WriteLine($"Crawling API...");
        try {
            var pmpApiClient = _pmpApiFactory.CreateApiClient();
            var resourceFilePath = GetCacheFilename("Resources");
            var resourceFileTempPath = $"{resourceFilePath}.tmp";

            List<Resource> resources = new List<Resource>();
            await foreach (var resource in pmpApiClient.GetAllResourcesAsync()) {
                resources.Add(resource);
            }
            _cache.UpdateResources(resources);
            using (FileStream fs = File.Open(resourceFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                JsonSerializer.Serialize<List<Resource>>(fs, resources, new JsonSerializerOptions { WriteIndented = true });
            }
            File.Move(resourceFileTempPath, resourceFilePath, true);
        } catch (Exception e) {
            _logger.LogError(e, $"Error crawling API");
        }
    }

    private async Task CrawlSql() {
        Console.WriteLine($"Crawling SQL...");
        try {
            var pmpSqlClient = _pmpApiFactory.CreateSqlClient();
            var rgFilePath = GetCacheFilename("ResourceGroups");
            var rgFileTempPath = $"{rgFilePath}.tmp";

            var rgs = new List<ResourceGroup>();
            await foreach (var rg in pmpSqlClient.GetResourceGroupsAsync()) {
                rgs.Add(rg);
            }
            _cache.UpdateResourceGroups(rgs);
            using (FileStream fs = File.Open(rgFileTempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                JsonSerializer.Serialize<List<ResourceGroup>>(fs, rgs, new JsonSerializerOptions { WriteIndented = true });
            }
            File.Move(rgFileTempPath, rgFilePath, true);
        } catch (Exception e) {
            _logger.LogError(e, $"Error crawling SQL");
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