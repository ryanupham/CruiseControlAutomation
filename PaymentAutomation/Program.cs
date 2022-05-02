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
            var generateReportsTask = program.GenerateReports();
            await DisplayProcessingMessage(generateReportsTask);
            Console.WriteLine("\n\nFinished.");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Error: {e.Message}");
            Console.WriteLine("Please ensure you are logged in to Cruise Control");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
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
            .AddHttpClient()
            .AddSingleton<IPrintToPdfService>(new ChromePrintToPdfService(appSettings.ChromePath))
            .AddSingleton<IPayrollServiceFactory, PayrollServiceFactory>()
            .AddSingleton(
                sp => sp.GetRequiredService<IPayrollServiceFactory>().GetService()
            )
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

    private async Task GenerateReports()
    {
        var weekEndingDate = await GetUserSelectedWeekEndingDate();
        await payrollService.GenerateReportsForWeekEnding(weekEndingDate);
    }

    private static async Task DisplayProcessingMessage(Task task)
    {
        Console.WriteLine("\nProcessing");

        while (!task.IsCompleted)
        {
            Console.Write(".");
            await task.WaitAsync(TimeSpan.FromMilliseconds(750));
        }
    }
}
