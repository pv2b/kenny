namespace PmpApiClient;
using System.IO;
using System.Text.Json;
using System.Net.Http.Json;

public class PmpApiClient : BasePmpApiClient {
    Uri ApiBaseUri { get; }
    string AuthToken { get; }
    private static readonly HttpClient s_httpClient;
    private const int MAX_CONNECTIONS = 8;
    private CancellationToken _cancellationToken;

    static PmpApiClient() {
        var handler = new HttpClientHandler();
        handler.MaxConnectionsPerServer = MAX_CONNECTIONS;
        s_httpClient = new HttpClient(handler);
    }

    public PmpApiClient(Uri apiBaseUri, string authToken, CancellationToken cancellationToken = default) {
        ApiBaseUri = apiBaseUri;
        AuthToken = authToken;
        _cancellationToken = cancellationToken;
    }

    public static string FormatUri(FormattableString uri)
    {
        return string.Format(
            uri.Format, 
            uri.GetArguments()
                .Select(a => Uri.EscapeDataString(a?.ToString() ?? ""))
                .ToArray());
    }

    private async Task<T?> ApiGetAsync<T>(FormattableString relativeUriFS) {
        var uri = new Uri(ApiBaseUri, FormatUri(relativeUriFS));
        using (var request = new HttpRequestMessage(HttpMethod.Get, uri)) {
            request.Headers.Add("AUTHTOKEN", AuthToken);
            var response = await s_httpClient.SendAsync(request, _cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
 
    override public Task<ApiResponse<IEnumerable<ResourceSummary>>?> GetAllResourceSummaryApiResponseAsync() {
        return ApiGetAsync<ApiResponse<IEnumerable<ResourceSummary>>>($"restapi/json/v1/resources");
    }

    override public Task<ApiResponse<ResourceDetails>?> GetResourceDetailsApiResponseAsync(String resourceId) {
        return ApiGetAsync<ApiResponse<ResourceDetails>>($"restapi/json/v1/resources/{resourceId}/accounts");
    }

    override public Task<ApiResponse<AccountPassword>?> GetAccountPasswordApiResponseAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request) {
        return ApiGetAsync<ApiResponse<AccountPassword>>($"restapi/json/v1/resources/{resourceId}/accounts/{accountId}/password");
    }
}