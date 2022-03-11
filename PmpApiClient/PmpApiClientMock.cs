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
}