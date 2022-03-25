using PmpApiClient;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

public class PmpApiClientMock : BasePmpApiClient {
    private async Task<T?> DeserializeFromFileAsync<T>(String filename) {
        T? response;
        using (FileStream fs = File.Open(filename, FileMode.Open)) {
            response = await JsonSerializer.DeserializeAsync<T>(fs);
        }
        return response;
    }

    override public Task<string> GetAllResourceSummaryJsonAsync() {
        return File.ReadAllTextAsync(@"json\resources1.json");
    }

    override public Task<string> GetResourceAssociatedGroupsJsonAsync(string resourceId) {
        return File.ReadAllTextAsync(@"json\groups1.json");
    }

    override public Task<string> GetResourceDetailsJsonAsync(String resourceId) {
        if (!resourceId.Equals("303")) {
            throw new NotImplementedException();
        }
        return File.ReadAllTextAsync(@"json\accounts1.json");
    }

    override public Task<string> GetAccountPasswordJsonAsync(string resourceId, string accountId, ApiRequest<PasswordRequestDetails> request) {
        if (!resourceId.Equals("303") || !accountId.Equals("307")) {
            throw new NotImplementedException();
        }
        return File.ReadAllTextAsync(@"json\password1.json");
    }
}