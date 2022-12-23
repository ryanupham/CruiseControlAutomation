using System.Text.Json.Serialization;

namespace PaymentAutomation.Models.ApiResponses
{
    internal record CommissionDetailReportResponse
    {
        [JsonPropertyName("coneCommisonTrackingHistoryDetailDtos")]
        public Dictionary<string, Dictionary<string, Dictionary<string, IEnumerable<Booking>>>> ConeCommisonTrackingHistoryDetailDtos { get; init; } = new();
        [JsonPropertyName("adjustmentAmountsDtos")]
        public Dictionary<string, IEnumerable<Adjustment>> AdjustmentAmountsDtos { get; init; } = new();
        [JsonPropertyName("valid")]
        public bool Valid { get; init; }
    }
}
