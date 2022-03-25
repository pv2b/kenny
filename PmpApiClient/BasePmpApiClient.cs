namespace PmpApiClient;

using System;
using System.Collections.Concurrent;
using System.Text.Json;

public abstract class BasePmpApiClient {
    private T HandleApiResponse<T>(string json, string operationDescription) {
        ApiResponse<T>? response;
        try {
            response = JsonSerializer.Deserialize<ApiResponse<T>>(json);
        } catch (Exception inner) {
            throw new Exception($"Error when {operationDescription}: Json Deserialization error", inner);
        }
        if (response == null) {
            throw new Exception($"Error when {operationDescription}: Empty API response");
        }
        if (!response.Operation.Result.Status.Equals("success", StringComparison.CurrentCultureIgnoreCase)) {
            throw new Exception($"Error when {operationDescription}: {response.Operation.Result.Message}");
        }
        return response.Operation.Details;
    }

    public abstract Task<string> GetAllResourceSummaryJsonAsync();
    public async Task<IEnumerable<ResourceSummary>> GetAllResourceSummaryAsync() {
        var json = await GetAllResourceSummaryJsonAsync();
        return HandleApiResponse<IEnumerable<ResourceSummary>>(json, $"getting all resource summaries");
    }

    public abstract Task<string> GetResourceAssociatedGroupsJsonAsync(string resourceId);
    public async Task<IEnumerable<ResourceGroupSummary>> GetResourceAssociatedGroupsAsync(string resourceId) {
        var json = await GetResourceAssociatedGroupsJsonAsync(resourceId);
        return HandleApiResponse<AssociatedGroupContainer>(json, $"getting resource associated groups for resource {resourceId}")?.Groups ?? Enumerable.Empty<ResourceGroupSummary>();
    }
    public Task<IEnumerable<ResourceGroupSummary>> GetResourceAssociatedGroupsAsync(ResourceSummary resourceSummary) {
        if (resourceSummary.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceAssociatedGroupsAsync(resourceSummary.Id);
    }

    public abstract Task<string> GetResourceDetailsJsonAsync(string resourceId);
    public async Task<ResourceDetails> GetResourceDetailsAsync(string resourceId) {
        var json = await GetResourceDetailsJsonAsync(resourceId);
        return HandleApiResponse<ResourceDetails>(json, $"getting resource details for resource {resourceId}");
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

    public abstract Task<string> GetAccountPasswordJsonAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request);
    public async Task<AccountPassword> GetAccountPasswordAsync(string resourceId, string accountId, string? reason = null, string? ticketId = null) {
        var passwordRequestDetails = new PasswordRequestDetails(reason, ticketId);
        var request = new ApiRequest<PasswordRequestDetails>(passwordRequestDetails);
        var json = await GetAccountPasswordJsonAsync(resourceId, accountId, request);
        return HandleApiResponse<AccountPassword>(json, @"getting account password for resource {resourceId} and account {accountId}");
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