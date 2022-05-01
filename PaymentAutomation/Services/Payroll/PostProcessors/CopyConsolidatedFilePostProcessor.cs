using PaymentAutomation.Models;

namespace PaymentAutomation.Services.Payroll;

internal class CopyConsolidatedFilePostProcessor : IPayrollPostProcessor
{
    private readonly string outputFolder;

    public CopyConsolidatedFilePostProcessor(string outputFolder) =>
        this.outputFolder = outputFolder;

    public void Process(string filepath, DateOnly weekEndingDate, Agent? agent)
    {
        var filename = Path.GetFileName(filepath);
        var destinationFilepath = Path.Combine(outputFolder, "Consolidated Reports", filename);
        var destinationDirectory = Path.GetDirectoryName(destinationFilepath)!;

        Directory.CreateDirectory(destinationDirectory);
        File.Copy(filepath, destinationFilepath, true);
    }
}
