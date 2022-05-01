using PaymentAutomation.Models;

namespace PaymentAutomation.Services.Payroll;

internal class CopyAgentFilePostProcessor : IPayrollPostProcessor
{
    private readonly string outputFolder;

    public CopyAgentFilePostProcessor(string outputFolder) =>
        this.outputFolder = outputFolder;

    public void Process(string filepath, DateOnly weekEndingDate, Agent? agent)
    {
        var filename = Path.GetFileName(filepath);
        var destinationFilepath = Path.Combine(outputFolder, agent?.FullName ?? "", filename);
        var destinationDirectory = Path.GetDirectoryName(destinationFilepath)!;

        Directory.CreateDirectory(destinationDirectory);
        File.Copy(filepath, destinationFilepath, true);
    }
}
