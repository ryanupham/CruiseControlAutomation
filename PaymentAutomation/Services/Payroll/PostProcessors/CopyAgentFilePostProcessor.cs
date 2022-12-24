namespace PaymentAutomation.Services.Payroll;

internal class CopyAgentFilePostProcessor : IPayrollPostProcessor
{
    private readonly string outputFolder;

    public CopyAgentFilePostProcessor(string outputFolder) =>
        this.outputFolder = outputFolder;

    public void Process(ReportMetadata reportMetadata)
    {
        if (reportMetadata.ReportType is not ReportType.Agent) return;

        var filename = Path.GetFileName(reportMetadata.Filepath);
        var destinationFilepath = Path.Combine(outputFolder, reportMetadata.Agent?.FullName ?? "", filename);
        var destinationDirectory = Path.GetDirectoryName(destinationFilepath)!;

        Directory.CreateDirectory(destinationDirectory);
        File.Copy(reportMetadata.Filepath, destinationFilepath, true);
    }
}
