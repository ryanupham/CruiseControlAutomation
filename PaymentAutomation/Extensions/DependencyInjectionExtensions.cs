using CruiseControl.DataAccess;
using CruiseControl.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PaymentAutomation.Models;
using PaymentAutomation.Services;
using PaymentAutomation.Services.Payroll;
using RazorLight;

namespace PaymentAutomation.Extensions;
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRazorEngine(this IServiceCollection services)
    {
        var templateDirectory = Path.Combine(Environment.CurrentDirectory, "Templates");
        var razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templateDirectory)
            .Build();

        return services.AddSingleton<IRazorLightEngine>(razorEngine);
    }

    public static IServiceCollection AddReportingApiClient(this IServiceCollection services, AppSettings appSettings)
    {
        var agentSettingsProvider = new AgentSettingsProvider(appSettings.Agents);  // TODO: replace with AgentConfigurations (extends ReadOnlyDictionary<string, AgentSettings>)

        return services.AddSingleton<IReportingApiClient>(
            sp => new ReportingApiClient(
                appSettings.Api.AgencyId,
                sp.GetRequiredService<HttpClient>(),
                agentSettingsProvider));
    }

    public static IServiceCollection AddAgentRolloverService(this IServiceCollection services)
    {
        var filePath = UserFileHelper.InitializeFile("agentBalanceByWeek.json");
        return services
            .AddSingleton<ISimpleDataStore<AgentRolloverCollection>>(
                new JsonFileDataStore<AgentRolloverCollection>(
                    new FileTextDataStore(filePath)))
            .AddSingleton<IAgentRolloverService, AgentRolloverService>();
    }

    public static IServiceCollection AddPrintToPdfService(this IServiceCollection services, string chromePath) =>
        services.AddSingleton<IPrintToPdfService>(
            IsDryRun
                ? new NullPrintToPdfService()
                : new ChromePrintToPdfService(chromePath));

    public static IServiceCollection AddPayrollService(this IServiceCollection services, AppSettings appSettings)
    {
        var manager = appSettings.Agents.Values.Single(a => a.IsManager)!;

        var shouldEmail = !(IsDryRun || IsNoEmail);
        if (shouldEmail)
        {
            services
                .AddSingleton<IPayrollPostProcessor>(
                    new CopyConsolidatedFilePostProcessor(appSettings.OutputFolder)
                ).AddSingleton<IPayrollPostProcessor>(
                    new EmailConsolidatedReportPostProcessor(
                        appSettings.Email,
                        manager.Name,
                        manager.Email,
                        manager.Name,
                        manager.Email
                    )
                ).AddSingleton<IPayrollPostProcessor>(
                    new CopyAgentFilePostProcessor(appSettings.OutputFolder)
                ).AddSingleton<IPayrollPostProcessor>(
                    new EmailAgentReportPostProcessor(
                        appSettings.Email,
                        manager.Name,
                        manager.Email,
                        manager.Name,
                        manager.Email
                    )
                );
        }

        return services.AddSingleton<IPayrollService, PayrollService>();
    }

    private static bool IsDryRun =>
        Environment.GetCommandLineArgs().Contains("--dry-run");
    private static bool IsNoEmail =>
        Environment.GetCommandLineArgs().Contains("--no-email");
}
