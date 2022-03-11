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

}