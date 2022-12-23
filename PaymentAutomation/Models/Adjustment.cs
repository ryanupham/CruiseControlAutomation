using PaymentAutomation.Enums;
using PaymentAutomation.JsonConverters;
using System.Text.Json.Serialization;

namespace PaymentAutomation.Models;

public record Adjustment
{
    [JsonPropertyName("commTypeDesc")]
    public AdjustmentType Type { get; init; } = AdjustmentType.Unknown;
    public Agent Agent { get; init; } = new AgentBookingRecord();
    [JsonPropertyName("recordDesc")]
    public string Description { get;    init; } = string.Empty;
    [JsonPropertyName("weekEndingDate")]
    public DateOnly WeekEndingDate { get; init; }
    [JsonPropertyName("franchiseDueAmt")]
    public decimal FranchiseDueAmount { get; init; }
    [JsonPropertyName("franchisePaidAmt")]
    public decimal AmountPaidPrior { get; init; }
    [JsonPropertyName("franchisePayable")]
    public decimal FranchisePayable { get; init; }
    [JsonPropertyName("svcFeeAmt")]
    public decimal ServiceFeeAmount { get; init; }
    [JsonPropertyName("svcFeePaid")]
    public decimal ServiceFeePaid { get; init; }
    [JsonPropertyName("svcFeePayable")]
    public decimal ServiceFeePayable { get; init; }
    [JsonPropertyName("totalCommission")]
    public decimal CommissionTotalAmount { get; init; }
    [JsonPropertyName("totalFranchiseAmt")]
    public decimal CommissionFranchiseAmount { get; init; }
    [JsonPropertyName("valid")]
    public bool Valid { get; init; }
}
