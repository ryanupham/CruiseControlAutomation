using CruiseControl.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentAutomation.Extensions;
using PaymentAutomation.Models;
using PaymentAutomation.Services;
using PaymentAutomation.Utilities;
using PaymentAutomation.Utilities.ConsoleOptions;
using System.Text.Json;

namespace PaymentAutomation;

public class Program
{
    private readonly IReportingApiClient reportingApiClient;
    private readonly IPayrollService payrollService;

    public Program(IReportingApiClient reportingApiClient, IPayrollService payrollService)
    {
        this.reportingApiClient = reportingApiClient;
        this.payrollService = payrollService;
    }

    public static async Task Main()
    {
        try
        {
            await Run();
        }
        catch (JsonException e)
        {
            Console.WriteLine($"\n\n{e}");

            Console.WriteLine($"\n\nError: {e.Message}");
            Console.WriteLine("Please ensure you are logged in to Cruise Control");
        }
        catch (Exception e)
        {
            Console.WriteLine($"\n\n{e}");
            Console.WriteLine($"\n\nError: {e.Message}");
        }
        
        Console.ReadKey();
    }

    private static async Task Run()
    {
        var appSettings = GetAppSettings();
        var serviceProvider = GetServiceProvider(appSettings);
        var program = serviceProvider.GetRequiredService<Program>();
        var weekEndingDate = await program.GetUserSelectedWeekEndingDate();

        Console.WriteLine("\n\nProcessing...");
        await program.GenerateReports(weekEndingDate);
        Console.WriteLine("\n\nFinished.");
    }

    private static AppSettings GetAppSettings()
    {
        var appsettingsFileName = "appsettings.json";
        var isFileInitialized = UserFileLoader.IsFileInitialized(appsettingsFileName);
        var appsettingsFilePath = UserFileLoader.LoadFilePath(appsettingsFileName);
        var appsettingsFileDirectory = Path.GetDirectoryName(appsettingsFilePath)
            ?? throw new Exception($"Unable to get directory for {appsettingsFilePath}");
        if (!isFileInitialized)
        {
            Console.WriteLine($"Please fill out the settings file at {appsettingsFilePath}");
            Console.WriteLine("When finished, press any key to continue...");
            Console.ReadKey();
        }

        return new ConfigurationBuilder()
            .SetBasePath(appsettingsFileDirectory)
            .AddJsonFile(appsettingsFileName)
            .Build()
            .Get<AppSettings>()
            ?? throw new Exception($"Unable to load {appsettingsFileName}");
    }

    private static ServiceProvider GetServiceProvider(AppSettings appSettings) =>
        new ServiceCollection()
            .AddRazorEngine()
            .AddAgentRolloverRepository()
            .AddCruiseControlHttpClient(appSettings.Api.BaseUrl)
            .AddReportingApiClient(appSettings)
            .AddPrintToPdfService(appSettings)
            .AddPayrollService(appSettings)
            .AddSingleton<IRolloverService, RolloverService>()
            .AddSingleton<Program>()
            .BuildServiceProvider();

    private async Task<List<DateOnly>> GetWeekEndingDates() =>
        (await reportingApiClient.GetWeekEndingDates())
            .OrderByDescending(d => d)
            .ToList();

    private async Task<DateOnly> GetUserSelectedWeekEndingDate()
    {
        var weekEndingDates = await GetWeekEndingDates();
        var dateOptions = new OptionList<DateOnly>();
        dateOptions.AddRange(
            weekEndingDates
                .Select(d => new Option<DateOnly>(d.ToString(), d))
                .ToList()
        );
        var optionSelector = new ConsoleOptionSelector<DateOnly>(dateOptions, "Select the week ending date: ");

        return optionSelector.GetSelectedOption();
    }

    private async Task GenerateReports(DateOnly weekEndingDate) =>
        await payrollService.GenerateReportsForWeekEnding(weekEndingDate);
}
