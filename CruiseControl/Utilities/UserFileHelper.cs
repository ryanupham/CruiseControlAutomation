using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace CruiseControl.Utilities;

public static class UserFileHelper
{
    private static readonly string ApplicationDataDirectory =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Assembly.GetEntryAssembly()?.GetName().Name
                ?? throw new InvalidOperationException("Unable to load assembly name")
        );
    private static readonly string BundledFileDirectory =
        AppDomain.CurrentDomain.BaseDirectory;

    public static T LoadSettingsFile<T>(string fileName)
    {
        var isFileInitialized = IsFileInitialized(fileName);
        var filePath = InitializeFile(fileName);
        var fileDirectory = Path.GetDirectoryName(filePath)
            ?? throw new Exception($"Unable to get directory for {filePath}");
        if (!isFileInitialized)
        {
            Console.WriteLine($"Please fill out the settings file at {filePath}");
            Console.WriteLine("When finished, press any key to continue...");
            Console.ReadKey();
        }

        return new ConfigurationBuilder()
            .SetBasePath(fileDirectory)
            .AddJsonFile(fileName)
            .Build()
            .Get<T>()
            ?? throw new Exception($"Unable to load {fileName}");
    }

    public static bool IsFileInitialized(string fileName) =>
        File.Exists(GetApplicationDataFilePath(fileName));

    public static string InitializeFile(string fileName)
    {
        var filePath = GetApplicationDataFilePath(fileName);
        if (IsFileInitialized(fileName)) return filePath;

        var bundledFilePath = GetBundledFilePath(fileName);
        Directory.CreateDirectory(ApplicationDataDirectory);
        File.Copy(bundledFilePath, filePath);

        return filePath;
    }

    public static string GetApplicationDataFilePath(string fileName) =>
        Path.Combine(ApplicationDataDirectory, fileName);

    private static string GetBundledFilePath(string fileName) =>
        Path.Combine(BundledFileDirectory, fileName);
}
