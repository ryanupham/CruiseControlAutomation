using System.Diagnostics;

namespace PaymentAutomation.Services;
public interface IPrintToPdfService
{
    void PrintToPdf(string htmlFilePath, string outFilePath);
}

internal class ChromePrintToPdfService : IPrintToPdfService
{
    private readonly string chromePath;

    public ChromePrintToPdfService(string chromePath) =>
        this.chromePath = chromePath;

    public void PrintToPdf(string htmlFilePath, string outFilePath)
    {
        EnsureDirectoryExistsForOutputFile(outFilePath);

        using var process = new Process();
        process.StartInfo.FileName = chromePath;
        process.StartInfo.Arguments = @$"--headless --disable-gpu --print-to-pdf-no-header --print-to-pdf=""{outFilePath}"" {htmlFilePath}";
        process.Start();
        process.WaitForExit();
    }

    private static void EnsureDirectoryExistsForOutputFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);
    }
}

internal class NullPrintToPdfService : IPrintToPdfService
{
    public void PrintToPdf(string htmlFilePath, string outFilePath) { }
}
