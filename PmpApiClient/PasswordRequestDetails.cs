using System.Text.Json.Serialization;

namespace PmpApiClient;

public class PasswordRequestDetails {
    public PasswordRequestDetails(string? reason = null, string? ticketId = null) {
        Reason = reason;
        TicketId = ticketId;
    }

    [JsonPropertyName("REASON")]
    public string? Reason { get; }

    [JsonPropertyName("TICKETID")]
    public string? TicketId { get; }
}