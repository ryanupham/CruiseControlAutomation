using System.Text.Json.Serialization;

namespace PaymentAutomation.Models;
public record AgentListRecord : Agent
{
    [JsonPropertyName("username")]
    public override string Id { get; init; } = string.Empty;
    [JsonPropertyName("firstName")]
    public override string FirstName { get; init; } = string.Empty;
    [JsonPropertyName("lastName")]
    public override string LastName { get; init; } = string.Empty;
    [JsonPropertyName("fullName")]
    public override string FullName => $"{FirstName} {LastName}";
    public override AgentSettings Settings { get; init; } = new();
}
