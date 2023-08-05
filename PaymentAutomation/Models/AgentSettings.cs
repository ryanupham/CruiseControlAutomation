namespace PaymentAutomation.Models;
public record AgentSettings
{
    public string Name { get; init; } = string.Empty;
    public decimal CommissionFeePercent { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool IsManager { get; init; }
}
