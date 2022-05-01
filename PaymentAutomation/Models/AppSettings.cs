namespace PaymentAutomation.Models;

public record AppSettings
{
    public Dictionary<string, AgentSettings> Agents { get; init; } = new();
    public EmailConfiguration Email { get; init; } = new();
    public ApiConfiguration Api { get; init; } = new();
    public string ChromePath { get; init; } = string.Empty;
    public string OutputFolder { get; init; } = string.Empty;
}
