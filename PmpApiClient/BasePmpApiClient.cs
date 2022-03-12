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

    public abstract ApiResponse<IEnumerable<Resource>>? GetResourcesApiResponse();
    public IEnumerable<Resource> GetResources() {
        var response = GetResourcesApiResponse();
        return HandleApiResponse<IEnumerable<Resource>>(response);
    }

    public abstract ApiResponse<ResourceAccountList>? GetResourceAccountListApiResponse(string resourceId);
    public ResourceAccountList GetResourceAccountList(string resourceId) {
        var response = GetResourceAccountListApiResponse(resourceId);
        return HandleApiResponse<ResourceAccountList>(response);
    }
    public ResourceAccountList GetResourceAccountList(Resource resource) {
        if (resource.Id == null) {
            throw new Exception("Missing value for Id property");
        }
        return GetResourceAccountList(resource.Id);
   }

    public abstract ApiResponse<AccountPassword>? GetAccountPasswordApiResponse(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request);
    public AccountPassword GetAccountPassword(string resourceId, string accountId, string? reason = null, string? ticketId = null) {
        var passwordRequestDetails = new PasswordRequestDetails(reason, ticketId);
        var request = new ApiRequest<PasswordRequestDetails>(passwordRequestDetails);
        var response = GetAccountPasswordApiResponse(resourceId, accountId, request);
        return HandleApiResponse<AccountPassword>(response);
    }
    public AccountPassword GetAccountPassword(Resource resource, ResourceAccountList.Account account, string? reason = null, string? ticketId = null) {
        if (resource.Id == null) {
            throw new Exception("Missing value for resource Id property");
        }
        if (account.Id == null) {
            throw new Exception("Missing value for account Id property");
        }
        return GetAccountPassword(resource.Id, account.Id, reason, ticketId);
   }
}