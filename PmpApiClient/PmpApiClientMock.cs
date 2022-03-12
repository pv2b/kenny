namespace PmpApiClient;
using System.IO;
using System.Text.Json;

public class PmpApiClientMock : BasePmpApiClient {
    private async Task<T?> DeserializeFromFileAsync<T>(String filename) {
        T? response;
        using (FileStream fs = File.Open(filename, FileMode.Open)) {
            response = await JsonSerializer.DeserializeAsync<T>(fs);
        }
        return response;
    }

    override public Task<ApiResponse<IEnumerable<Resource>>?> GetResourcesApiResponseAsync() {
        return DeserializeFromFileAsync<ApiResponse<IEnumerable<Resource>>>(@"json\resources1.json");
    }

    override public Task<ApiResponse<ResourceAccountList>?> GetResourceAccountListApiResponseAsync(String resourceId) {
        if (!resourceId.Equals("303")) {
            throw new NotImplementedException();
        }
        return DeserializeFromFileAsync<ApiResponse<ResourceAccountList>>(@"json\accounts1.json");
    }

    override public Task<ApiResponse<AccountPassword>?> GetAccountPasswordApiResponseAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request) {
        if (!resourceId.Equals("303") || !accountId.Equals("307")) {
            throw new NotImplementedException();
        }
        return DeserializeFromFileAsync<ApiResponse<AccountPassword>>(@"json\password1.json");
    }
}