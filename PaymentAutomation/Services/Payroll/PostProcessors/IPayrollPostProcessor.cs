namespace PaymentAutomation.Services.Payroll;
public interface IPayrollPostProcessor
{
    public void Process(ReportMetadata reportMetadata);
}
