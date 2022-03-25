namespace PmpApiClient;

using System;
using System.Collections.Concurrent;

public abstract class BasePmpApiClient {
    private T HandleApiResponse<T>(ApiResponse<T>? response, string operationDescription) {
        if (response == null) {
            throw new Exception($"Error when {operationDescription}: Empty API response");
        }
        if (!response.Operation.Result.Status.Equals("success", StringComparison.CurrentCultureIgnoreCase)) {
            throw new Exception($"Error when {operationDescription}: {response.Operation.Result.Message}");
        }
        return response.Operation.Details;
    }

    public abstract Task<ApiResponse<IEnumerable<ResourceSummary>>?> GetAllResourceSummaryApiResponseAsync();
    public async Task<IEnumerable<ResourceSummary>> GetAllResourceSummaryAsync() {
        var response = await GetAllResourceSummaryApiResponseAsync();
        return HandleApiResponse<IEnumerable<ResourceSummary>>(response, $"getting all resource summaries");
    }

    public abstract Task<ApiResponse<AssociatedGroupContainer>?> GetResourceAssociatedGroupsApiResponseAsync(string resourceId);
    public async Task<IEnumerable<ResourceGroupSummary>> GetResourceAssociatedGroupsAsync(string resourceId) {
        var response = await GetResourceAssociatedGroupsApiResponseAsync(resourceId);
        return HandleApiResponse<AssociatedGroupContainer>(response, $"getting resource associated groups for resource {resourceId}")?.Groups ?? Enumerable.Empty<ResourceGroupSummary>();
    }
    public Task<IEnumerable<ResourceGroupSummary>> GetResourceAssociatedGroupsAsync(ResourceSummary resourceSummary) {
        if (resourceSummary.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceAssociatedGroupsAsync(resourceSummary.Id);
    }

    public abstract Task<ApiResponse<ResourceDetails>?> GetResourceDetailsApiResponseAsync(string resourceId);
    public async Task<ResourceDetails> GetResourceDetailsAsync(string resourceId) {
        var response = await GetResourceDetailsApiResponseAsync(resourceId);
        return HandleApiResponse<ResourceDetails>(response, $"getting resource details for resource {resourceId}");
    }
    public Task<ResourceDetails> GetResourceDetailsAsync(ResourceSummary resourceSummary) {
        if (resourceSummary.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceDetailsAsync(resourceSummary.Id);
    }

    public async IAsyncEnumerable<Resource> GetAllResourcesAsync() {
        var summaries = await GetAllResourceSummaryAsync();
        var resources = new ConcurrentBag<Resource>();

        ParallelOptions parallelOptions = new() {
            MaxDegreeOfParallelism = 8
        };
        await Parallel.ForEachAsync(summaries, parallelOptions, async (summary, token) =>
        {
            var details = (summary.NoOfAccounts > 0) ? await GetResourceDetailsAsync(summary) : null;
            var groups = await GetResourceAssociatedGroupsAsync(summary);
            lock (resources) {
                resources.Add(new Resource(summary, details, groups));
            }
        });

        /* this doesn't need to be async enumerable but I didn't want to change calling code... */
        foreach (var resource in resources.OrderBy(r => r.Summary.Id)) {
            yield return resource;
        }
    }

    public abstract Task<ApiResponse<AccountPassword>?> GetAccountPasswordApiResponseAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request);
    public async Task<AccountPassword> GetAccountPasswordAsync(string resourceId, string accountId, string? reason = null, string? ticketId = null) {
        var passwordRequestDetails = new PasswordRequestDetails(reason, ticketId);
        var request = new ApiRequest<PasswordRequestDetails>(passwordRequestDetails);
        var response = await GetAccountPasswordApiResponseAsync(resourceId, accountId, request);
        return HandleApiResponse<AccountPassword>(response, @"getting account password for resource {resourceId} and account {accountId}");
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