namespace PaymentAutomation.Services.Payroll;

internal class CopyConsolidatedFilePostProcessor : IPayrollPostProcessor
{
    private readonly string outputFolder;

    public CopyConsolidatedFilePostProcessor(string outputFolder) =>
        this.outputFolder = outputFolder;

    public void Process(ReportMetadata reportMetadata)
    {
        if (reportMetadata.ReportType is not ReportType.Consolidated) return;

        var filename = Path.GetFileName(reportMetadata.Filepath);
        var destinationFilepath = Path.Combine(
            outputFolder,
            "Consolidated Reports",
            filename);
        var destinationDirectory = Path.GetDirectoryName(destinationFilepath)!;

        Directory.CreateDirectory(destinationDirectory);
        File.Copy(reportMetadata.Filepath, destinationFilepath, true);
    }
}
