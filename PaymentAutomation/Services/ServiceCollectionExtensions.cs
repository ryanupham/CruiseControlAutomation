using Microsoft.Extensions.DependencyInjection;
using PaymentAutomation.DataAccess;
using PaymentAutomation.Models;
using PaymentAutomation.Utilities;
using RazorLight;

namespace PaymentAutomation.Services;

internal static class ServiceCollectionExtensions
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

        var cookies = ChromeCookieManager.GetCookies(appSettings.Api.BaseUrl);
        CruiseControlCookies ccCookies;

        try
        {
            ccCookies = CruiseControlCookies.FromCookies(cookies);
        }
        catch (ArgumentException e)
        {
            throw new Exception("Unable to load cookies. Please ensure you are logged in to Cruise Control in Chrome.", e);
        }

        return services.AddSingleton<IReportingApiClient>(
            sp => new ReportingApiClient(
                new($"https://{appSettings.Api.BaseUrl}"),
                appSettings.Api.AgencyId,
                sp.GetRequiredService<IHttpClientFactory>(),
                ccCookies,
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
}
