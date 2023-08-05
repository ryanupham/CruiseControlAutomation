namespace PaymentAutomation.Models;
public record ApiConfiguration
{
    public string BaseUrl { get; init; } = string.Empty;
    public long AgencyId { get; init; }
}
