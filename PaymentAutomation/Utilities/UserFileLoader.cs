namespace PaymentAutomation.Utilities;

internal static class UserFileLoader
{
    private static readonly string ApplicationDataDirectory =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PaymentAutomation"
        );
    private static readonly string BundledFileDirectory =
        AppDomain.CurrentDomain.BaseDirectory;

    public static string LoadFilePath(string fileName)
    {
        InitializeFile(fileName);
        return GetApplicationDataFilePath(fileName);
    }

    public static bool IsFileInitialized(string fileName) =>
        File.Exists(GetApplicationDataFilePath(fileName));

    private static void InitializeFile(string fileName)
    {
        var filePath = GetApplicationDataFilePath(fileName);
        if (File.Exists(filePath)) return;

        var bundledFilePath = GetBundledFilePath(fileName);
        Directory.CreateDirectory(ApplicationDataDirectory);
        File.Copy(bundledFilePath, filePath);
    }

    private static string GetApplicationDataFilePath(string fileName) =>
        Path.Combine(ApplicationDataDirectory, fileName);

    private static string GetBundledFilePath(string fileName) =>
        Path.Combine(BundledFileDirectory, fileName);
}
