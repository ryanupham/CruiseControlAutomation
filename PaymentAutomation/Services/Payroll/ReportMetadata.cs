using PaymentAutomation.Models;

namespace PaymentAutomation.Services.Payroll;
public record ReportMetadata
{
    public ReportType ReportType { get; init; }
    public string Filepath { get; init; } = "";
    public DateOnly WeekEndingDate { get; init; }
    public Agent? Agent { get; init; }
}
