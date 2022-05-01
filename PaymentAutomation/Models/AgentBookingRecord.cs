using System.Text.Json.Serialization;

namespace PaymentAutomation.Models;

public record AgentBookingRecord : Agent
{
    [JsonPropertyName("agentOfRecord")]
    public override string Id { get; init; } = string.Empty;
    [JsonPropertyName("agentFirstName")]
    public override string FirstName { get; init; } = string.Empty;
    [JsonPropertyName("agentLastName")]
    public override string LastName { get; init; } = string.Empty;
    public override AgentSettings Settings { get; init; } = new();
    public override string FullName => $"{FirstName} {LastName}";
}
