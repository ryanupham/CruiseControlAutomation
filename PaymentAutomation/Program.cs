using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentAutomation.DataAccess;
using PaymentAutomation.Models;
using PaymentAutomation.Services;
using PaymentAutomation.Services.Payroll;
using PaymentAutomation.Utilities;
using PaymentAutomation.Utilities.ConsoleOptions;
using RazorLight;

var appSettings = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build()
    .Get<AppSettings>();

var templateDirectory = Path.Combine(Environment.CurrentDirectory, "Templates");
var razorEngine = new RazorLightEngineBuilder()
    .UseFileSystemProject(templateDirectory)
    .Build();

var cookies = ChromeCookieManager.GetCookies(appSettings.Api.BaseUrl);
CruiseControlCookies ccCookies;

try
{
    ccCookies = CruiseControlCookies.FromCookies(cookies);
}
catch (ArgumentException)
{
    Console.WriteLine("Unable to load cookies. Please ensure you are logged in to Cruise Control in Chrome.");

    Console.ReadKey();
    return;
}

var agentSettingsProvider = new AgentSettingsProvider(appSettings.Agents);
var agentRolloverRepository = new AgentRolloverRepository("agentBalanceByWeek.json");

var services = new ServiceCollection()
    .AddSingleton(appSettings)
    .AddSingleton<IRazorLightEngine>(razorEngine)
    .AddSingleton<IPrintToPdfService>(new ChromePrintToPdfService(appSettings.ChromePath))
    .AddSingleton<IReportingApiClient>(
        sp => new ReportingApiClient(
            new($"https://{appSettings.Api.BaseUrl}"),
            appSettings.Api.AgencyId,
            sp.GetRequiredService<IHttpClientFactory>(),
            ccCookies,
            agentSettingsProvider
        )
    )
    .AddSingleton<IPayrollServiceFactory, PayrollServiceFactory>()
    .AddSingleton(
        sp => sp.GetRequiredService<IPayrollServiceFactory>().GetService()
    )
    .AddSingleton<IRolloverService, RolloverService>()
    .AddSingleton<IAgentSettingsProvider>(agentSettingsProvider)
    .AddSingleton<IRepository<(DateOnly weekEndingDate, string agentId), decimal>>(agentRolloverRepository)
    .AddHttpClient();
var serviceProvider = services.BuildServiceProvider();

var reportingApiClient = serviceProvider.GetRequiredService<IReportingApiClient>()!;
var payrollService = serviceProvider.GetRequiredService<IPayrollService>()!;

var weekEndingDates = (await reportingApiClient.GetWeekEndingDates())
    .OrderByDescending(d => d)
    .ToList();

var dateOptions = new OptionList<DateOnly>();
dateOptions.AddRange(
    weekEndingDates
        .Select(d => new Option<DateOnly>(d.ToString(), d))
        .ToList()
);
var optionSelector = new ConsoleOptionSelector<DateOnly>(dateOptions, "Select the week ending date: ");

var weekEndingDate = optionSelector.GetSelectedOption();
var generateReportsTask = payrollService.GenerateReportsForWeekEnding(weekEndingDate);

_ = Task.Run(async () =>
{
    Console.WriteLine("\nWorking");

    while (!generateReportsTask.IsCompleted)
    {
        Console.Write(".");
        await Task.Delay(750);
    }
});

await generateReportsTask;

//foreach (var weekEndingDate in weekEndingDates.Reverse<DateOnly>())
//{
//    await payrollService.GenerateReportsForWeekEnding(weekEndingDate);
//    Console.WriteLine(weekEndingDate);
//}

Console.WriteLine("\n\nFinished.");
Console.ReadKey();
