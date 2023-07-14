using Microsoft.Extensions.DependencyInjection;
using PaymentAutomation.DataAccess;
using PaymentAutomation.Models;
using PaymentAutomation.Services;
using PaymentAutomation.Services.Payroll;
using PaymentAutomation.Utilities;
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
        var agentSettingsProvider = new AgentSettingsProvider(appSettings.Agents);

        return services.AddSingleton<IReportingApiClient>(
            sp => new ReportingApiClient(
                new($"https://{appSettings.Api.BaseUrl}"),
                appSettings.Api.AgencyId,
                sp.GetRequiredService<HttpClient>(),
                agentSettingsProvider
            )
        );
    }

    public static IServiceCollection AddAgentRolloverRepository(this IServiceCollection services)
    {
        var agentBalanceByWeekFilePath = UserFileLoader.LoadFilePath("agentBalanceByWeek.json");
        var agentRolloverRepository = new AgentRolloverRepository(agentBalanceByWeekFilePath);

        return services.AddSingleton<IRepository<(DateOnly weekEndingDate, string agentId), decimal>>(agentRolloverRepository);
    }

    public static IServiceCollection AddPrintToPdfService(this IServiceCollection services, AppSettings appSettings) =>
        services.AddSingleton<IPrintToPdfService>(
            Environment.GetCommandLineArgs().Contains("--dry-run")
                ? new NullPrintToPdfService()
                : new ChromePrintToPdfService(appSettings.ChromePath)
        );

    public static IServiceCollection AddPayrollService(this IServiceCollection services, AppSettings appSettings)
    {
        var manager = appSettings.Agents.Values.Single(a => a.IsManager)!;

        var isDryRun = Environment.GetCommandLineArgs().Contains("--dry-run");
        var isNoEmail = Environment.GetCommandLineArgs().Contains("--no-email");
        var shouldEmail = !(isDryRun || isNoEmail);

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
}
