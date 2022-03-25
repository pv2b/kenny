namespace PmpApiClient;
using System.IO;
using System.Text.Json;
using System.Net.Http.Json;

public class PmpApiClient : BasePmpApiClient {
    Uri ApiBaseUri { get; }
    string AuthToken { get; }
    private HttpClient _httpClient;
    private CancellationToken _cancellationToken;

    public PmpApiClient(Uri apiBaseUri, string authToken, HttpClient httpClient, CancellationToken cancellationToken = default) {
        ApiBaseUri = apiBaseUri;
        AuthToken = authToken;
        _cancellationToken = cancellationToken;
        _httpClient = httpClient;
    }

    public static string FormatUri(FormattableString uri)
    {
        return string.Format(
            uri.Format, 
            uri.GetArguments()
                .Select(a => Uri.EscapeDataString(a?.ToString() ?? ""))
                .ToArray());
    }

    private async Task<string> ApiGetAsync(FormattableString relativeUriFS) {
        var uri = new Uri(ApiBaseUri, FormatUri(relativeUriFS));
        using (var request = new HttpRequestMessage(HttpMethod.Get, uri)) {
            request.Headers.Add("AUTHTOKEN", AuthToken);
            var response = await _httpClient.SendAsync(request, _cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(_cancellationToken);
        }
    }
 
    override public Task<string> GetAllResourceSummaryJsonAsync() {
        return ApiGetAsync($"restapi/json/v1/resources");
    }

    override public Task<string> GetResourceAssociatedGroupsJsonAsync(string resourceId) {
        return ApiGetAsync($"restapi/json/v1/resources/{resourceId}/associatedGroups");
    }

    override public Task<string> GetResourceDetailsJsonAsync(String resourceId) {
        return ApiGetAsync($"restapi/json/v1/resources/{resourceId}/accounts");
    }

    override public Task<string> GetAccountPasswordJsonAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request) {
        return ApiGetAsync($"restapi/json/v1/resources/{resourceId}/accounts/{accountId}/password");
    }
}