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

    public abstract Task<ApiResponse<IEnumerable<Resource>>?> GetResourcesApiResponseAsync();
    public async Task<IEnumerable<Resource>> GetResourcesAsync() {
        var response = await GetResourcesApiResponseAsync();
        return HandleApiResponse<IEnumerable<Resource>>(response);
    }

    public abstract Task<ApiResponse<ResourceAccountList>?> GetResourceAccountListApiResponseAsync(string resourceId);
    public async Task<ResourceAccountList> GetResourceAccountListAsync(string resourceId) {
        var response = await GetResourceAccountListApiResponseAsync(resourceId);
        return HandleApiResponse<ResourceAccountList>(response);
    }
    public Task<ResourceAccountList> GetResourceAccountListAsync(Resource resource) {
        if (resource.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceAccountListAsync(resource.Id);
   }

    public abstract Task<ApiResponse<AccountPassword>?> GetAccountPasswordApiResponseAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request);
    public async Task<AccountPassword> GetAccountPasswordAsync(string resourceId, string accountId, string? reason = null, string? ticketId = null) {
        var passwordRequestDetails = new PasswordRequestDetails(reason, ticketId);
        var request = new ApiRequest<PasswordRequestDetails>(passwordRequestDetails);
        var response = await GetAccountPasswordApiResponseAsync(resourceId, accountId, request);
        return HandleApiResponse<AccountPassword>(response);
    }
    public Task<AccountPassword> GetAccountPasswordAsync(Resource resource, ResourceAccountList.Account account, string? reason = null, string? ticketId = null) {
        if (resource.Id == null) {
            throw new Exception("Missing value for resource Id property");
        }
        if (account.Id == null) {
            throw new Exception("Missing value for account Id property");
        }
        return GetAccountPasswordAsync(resource.Id, account.Id, reason, ticketId);
   }
}