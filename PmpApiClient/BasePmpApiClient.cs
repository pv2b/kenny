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

    public abstract Task<ApiResponse<ResourceDetails>?> GetResourceDetailsApiResponseAsync(string resourceId);
    public async Task<ResourceDetails> GetResourceDetailsAsync(string resourceId) {
        var response = await GetResourceDetailsApiResponseAsync(resourceId);
        return HandleApiResponse<ResourceDetails>(response);
    }
    public Task<ResourceDetails> GetResourceDetailsAsync(ResourceSummary resourceSummary) {
        if (resourceSummary.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceDetailsAsync(resourceSummary.Id);
    }

    public async IAsyncEnumerable<Resource> GetAllResourcesAsync() {
        var summaries = await GetAllResourceSummaryAsync();
        var r = new List<(ResourceSummary, Task<ResourceDetails>)>();
        foreach (var summary in summaries) {
            r.Add((summary, GetResourceDetailsAsync(summary)));
        }
        foreach ((ResourceSummary summary, Task<ResourceDetails> detailsTask) in r) {
            ResourceDetails details;
            try {
                details = await detailsTask;
            } catch {
                continue;
            }
            yield return new Resource(details);
        }
    }

    public abstract Task<ApiResponse<AccountPassword>?> GetAccountPasswordApiResponseAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request);
    public async Task<AccountPassword> GetAccountPasswordAsync(string resourceId, string accountId, string? reason = null, string? ticketId = null) {
        var passwordRequestDetails = new PasswordRequestDetails(reason, ticketId);
        var request = new ApiRequest<PasswordRequestDetails>(passwordRequestDetails);
        var response = await GetAccountPasswordApiResponseAsync(resourceId, accountId, request);
        return HandleApiResponse<AccountPassword>(response);
    }
    public Task<AccountPassword> GetAccountPasswordAsync(ResourceSummary resource, ResourceDetails.Account account, string? reason = null, string? ticketId = null) {
        if (resource.Id == null) {
            throw new Exception("Missing value for resource Id property");
        }
        if (account.Id == null) {
            throw new Exception("Missing value for account Id property");
        }
        return GetAccountPasswordAsync(resource.Id, account.Id, reason, ticketId);
   }
}