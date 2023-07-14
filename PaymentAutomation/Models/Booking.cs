using System.Text.Json.Serialization;

namespace PaymentAutomation.Models;

public record Booking
{
    [JsonPropertyName("caseId")]
    public long BookingId { get; init; }
    public Agent Agent { get; init; } = new AgentBookingRecord();
    [JsonPropertyName("passengerName")]
    public string PassengerName { get; init; } = string.Empty;
    [JsonPropertyName("supplierDesc")]
    public string Supplier { get; init; } = string.Empty;
    [JsonPropertyName("sailDate")]
    public DateOnly TripStartDate { get; init; }
    [JsonPropertyName("bookDate")]
    public DateOnly TripBookDate { get; init; }
    [JsonPropertyName("commissionablePrice")]
    public decimal CommissionablePrice { get; init; }
    [JsonPropertyName("svcFeePct")]
    public decimal ServiceFeePercent { get; init; }
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
    [JsonPropertyName("franchisePaidAmt")]
    public decimal AmountPaidPrior { get; init; }
    [JsonPropertyName("franchisePayable")]
    public decimal FranchisePayable { get; init; }
    [JsonPropertyName("franchiseDueAmt")]
    public decimal FranchiseDueAmount { get; init; }
    [JsonPropertyName("valid")]
    public bool Valid { get; init; }
}
