using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PmpApiClient;

/*
 * The PMP API will occasionally return an empty string for the "accounts" property, so this converter is required to handle that case.
 */
public class ResourceAccountListJsonConverter : JsonConverter<IEnumerable<ResourceDetails.Account>>
{
    public override IEnumerable<ResourceDetails.Account>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        try {
            string str = JsonSerializer.Deserialize<string>(ref reader, options)!;
            if (!str.Equals("")) {
                throw new Exception("Unexepected non-empty string! {str}");
            }
            return null;
        } catch (JsonException) {
        }
        return JsonSerializer.Deserialize<IEnumerable<ResourceDetails.Account>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<ResourceDetails.Account> value, JsonSerializerOptions options) {
        JsonSerializer.Serialize<IEnumerable<ResourceDetails.Account>>(writer, value, options);
    }
}