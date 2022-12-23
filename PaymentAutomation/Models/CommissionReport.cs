using System.Text.Json.Serialization;

namespace PaymentAutomation.Models;

internal record CommissionReport
{
    [JsonPropertyName("coneCommisonTrackingHistoryDetailDtos")]
    public IReadOnlyCollection<Booking> Bookings { get; init; } = new List<Booking>();
    [JsonPropertyName("adjustmentAmountsDtos")]
    public IReadOnlyCollection<Adjustment> Adjustments { get; init; } = new List<Adjustment>();
    [JsonPropertyName("valid")]
    public bool Valid { get; init; }
}
