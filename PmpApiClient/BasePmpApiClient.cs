namespace PmpApiClient;

using System;

public abstract class BasePmpApiClient {
    private T HandleApiResponse<T>(ApiResponse<T>? response) {
        if (response == null) {
            throw new Exception("Empty API response");
        }
        if (!response.Operation.Result.Status.Equals("success", StringComparison.CurrentCultureIgnoreCase)) {
            throw new Exception(response.Operation.Result.Message);
        }
        return response.Operation.Details;
    }

    public abstract Task<ApiResponse<IEnumerable<ResourceSummary>>?> GetAllResourceSummaryApiResponseAsync();
    public async Task<IEnumerable<ResourceSummary>> GetAllResourceSummaryAsync() {
        var response = await GetAllResourceSummaryApiResponseAsync();
        return HandleApiResponse<IEnumerable<ResourceSummary>>(response);
    }

    public abstract Task<ApiResponse<Resource>?> GetResourceApiResponseAsync(string resourceId);
    public async Task<Resource> GetResourceAsync(string resourceId) {
        var response = await GetResourceApiResponseAsync(resourceId);
        return HandleApiResponse<Resource>(response);
    }
    public Task<Resource> GetResourceAsync(ResourceSummary resourceSummary) {
        if (resourceSummary.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceAsync(resourceSummary.Id);
    }

    public async IAsyncEnumerable<Resource> GetAllResourcesAsync() {
        var summaries = await GetAllResourceSummaryAsync();
        var r = new List<Task<Resource>>();
        foreach (var summary in summaries) {
            r.Add(GetResourceAsync(summary));
        }
        foreach (Task<Resource> detailsTask in r) {
            yield return await detailsTask;
        }
    }

    public abstract Task<ApiResponse<AccountPassword>?> GetAccountPasswordApiResponseAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request);
    public async Task<AccountPassword> GetAccountPasswordAsync(string resourceId, string accountId, string? reason = null, string? ticketId = null) {
        var passwordRequestDetails = new PasswordRequestDetails(reason, ticketId);
        var request = new ApiRequest<PasswordRequestDetails>(passwordRequestDetails);
        var response = await GetAccountPasswordApiResponseAsync(resourceId, accountId, request);
        return HandleApiResponse<AccountPassword>(response);
    }
    public Task<AccountPassword> GetAccountPasswordAsync(ResourceSummary resource, Resource.Account account, string? reason = null, string? ticketId = null) {
        if (resource.Id == null) {
            throw new Exception("Missing value for resource Id property");
        }
        if (account.Id == null) {
            throw new Exception("Missing value for account Id property");
        }
        return GetAccountPasswordAsync(resource.Id, account.Id, reason, ticketId);
   }
}