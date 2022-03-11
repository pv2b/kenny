namespace PmpApiClient;
using System.IO;
using System.Text.Json;

public class PmpApiClientMock : BasePmpApiClient {
    private T? DeserializeFromFile<T>(String filename) {
        T? response;
        using (FileStream fs = File.Open(filename, FileMode.Open)) {
            response = JsonSerializer.Deserialize<T>(fs);
        }
        return response;
    }

    override public ApiResponse<IEnumerable<Resource>>? GetResourcesApiResponse() {
        return DeserializeFromFile<ApiResponse<IEnumerable<Resource>>>(@"json\resources1.json");
    }

    override public ApiResponse<ResourceAccountList>? GetResourceAccountListApiResponse(String resourceId) {
        if (!resourceId.Equals("303")) {
            throw new NotImplementedException();
        }
        return DeserializeFromFile<ApiResponse<ResourceAccountList>>(@"json\accounts1.json");
    }

    override public ApiResponse<AccountPassword>? GetAccountPasswordApiResponse(string resourceId, string accountId) {
        if (!resourceId.Equals("303") || !accountId.Equals("307")) {
            throw new NotImplementedException();
        }
        return DeserializeFromFile<ApiResponse<AccountPassword>>(@"json\password1.json");
    }
}