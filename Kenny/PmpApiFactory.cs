using System.Text.Json.Serialization;
using System.Text.Json;
using PmpApiClient;
using PmpSqlClient;
using System.Security.Claims;

public class PmpApiFactory {
    public Uri ApiBaseUri { get; set; }
    public string ApiAuthToken { get; set; }
    public string SqlConnectionString { get; set; }
    private HttpClient _httpClient;

    public PmpApiFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration) {
        var section = configuration.GetSection("PmpApi");
        ApiBaseUri = new Uri(section.GetValue<string>("ApiBaseUri"));
        ApiAuthToken = section.GetValue<string>("ApiAuthToken");
        SqlConnectionString = section.GetValue<string>("SqlConnectionString");
        _httpClient = httpClientFactory.CreateClient();
    }

    public BasePmpApiClient CreateApiClient(CancellationToken cancellationToken = default) {
        return new PmpApiClient.PmpApiClient(ApiBaseUri, ApiAuthToken, _httpClient, cancellationToken);
    }

    public PmpSqlClient.PmpSqlClient CreateSqlClient() {
        return new PmpSqlClient.PmpSqlClient(SqlConnectionString);
    }
}