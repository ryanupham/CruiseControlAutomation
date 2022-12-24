using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentAutomation.Models;
using PaymentAutomation.Services;
using PaymentAutomation.Services.Payroll;
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
        var appSettings = GetAppSettings();
        var serviceProvider = GetServiceProvider(appSettings);
        var program = serviceProvider.GetRequiredService<Program>();

        try
        {
            var weekEndingDate = await program.GetUserSelectedWeekEndingDate();

            Console.WriteLine("\n\nProcessing...");
            await program.GenerateReports(weekEndingDate);
            Console.WriteLine("\n\nFinished.");

            //var weekEndingDates = await program.GetWeekEndingDates();
            //foreach (var weekEndingDate in weekEndingDates).OrderBy(d => d))
            //{
            //    Console.WriteLine($"Week {weekEndingDate}...");
            //    await program.GenerateReports(weekEndingDate);
            //}
        }
        catch (JsonException e)
        {
            Console.WriteLine($"\n\n{e}");  // TODO: log file

            Console.WriteLine($"\n\nError: {e.Message}");
            Console.WriteLine("Please ensure you are logged in to Cruise Control");
        }
        catch (Exception e)
        {
            Console.WriteLine($"\n\n{e}");  // TODO: log file

            Console.WriteLine($"\n\nError: {e.Message}");
        }

        Console.ReadKey();
    }

    private static AppSettings GetAppSettings()
    {
        var appsettingsFileName = "appsettings.json";
        var isFileInitialized = UserFileLoader.IsFileInitialized(appsettingsFileName);
        var appsettingsFilePath = UserFileLoader.LoadFilePath(appsettingsFileName);
        var appsettingsFileDirectory = Path.GetDirectoryName(appsettingsFilePath);
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
            .Get<AppSettings>();
    }

    private static ServiceProvider GetServiceProvider(AppSettings appSettings) =>
        new ServiceCollection()
            .AddSingleton(appSettings)
            .AddRazorEngine()
            .AddAgentRolloverRepository()
            .AddReportingApiClient(appSettings)
            .AddPrintToPdfService(appSettings)
            .AddHttpClient()
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
