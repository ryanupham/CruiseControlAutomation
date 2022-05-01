using PaymentAutomation.Models;

namespace PaymentAutomation.Services.Payroll;

public interface IPayrollPostProcessor
{
    public void Process(string filepath, DateOnly weekEndingDate, Agent? agent);
}
