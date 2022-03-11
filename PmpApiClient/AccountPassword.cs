using System.Text.Json.Serialization;

namespace PmpApiClient;

public class AccountPassword {
    public AccountPassword(string password) {
        Password = password;
    }

    [JsonPropertyName("PASSWORD")]
    public string Password { get; }
}
