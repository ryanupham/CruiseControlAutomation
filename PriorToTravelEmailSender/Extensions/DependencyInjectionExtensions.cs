using CruiseControl.DataAccess;
using CruiseControl.Extensions;
using CruiseControl.Services;
using CruiseControl.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PriorToTravelEmailSender.DataAccess;
using PriorToTravelEmailSender.Models;
using PriorToTravelEmailSender.Services;

namespace PriorToTravelEmailSender.Extensions;
internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddUpcomingBookingsEmailProcessor(
        this IServiceCollection services,
        AppSettings appSettings)
    {
        return services
            .AddSingleton<IUpcomingBookingsEmailProcessor, UpcomingBookingsEmailProcessor>()
            .AddBookingService(appSettings.Username)
            .AddBookingEmailService(appSettings.EmailOptions)
            .AddBookingNoteService()
            .AddCruiseControlHttpClient(appSettings.BaseUrl)
            .AddEmailService(appSettings.EmailConfiguration)
            .AddSingleton<IUserService, UserService>()
            .AddApplicationExecutionHistoryService("executionHistoryDetails.json");
    }

    private static IServiceCollection AddApplicationExecutionHistoryService(
        this IServiceCollection services,
        string dataFileName)
    {
        var filePath = UserFileHelper.InitializeFile(dataFileName);

        return services
            .AddSingleton<IApplicationExecutionHistoryService>(sp =>
            {
                var historyService =
                    sp.GetRequiredService<ApplicationExecutionHistoryService>();

                return IsDryRun
                    ? new ReadOnlyApplicationExecutionHistoryService(historyService)
                    : historyService;
            })
            .AddSingleton<ApplicationExecutionHistoryService>()
            .AddSingleton<ISimpleDataStore<ExecutionHistoryDetails>>(
                new ExecutionHistoryDetailsDataStore(
                    new FileTextDataStore(filePath)));
    }

    private static IServiceCollection AddBookingService(
        this IServiceCollection services,
        string username) =>
            services
                .AddSingleton(new BookingServiceSettings(username))
                .AddSingleton<IBookingService, BookingService>();

    private static IServiceCollection AddBookingEmailService(
        this IServiceCollection services,
        EmailOptions emailOptions) =>
            IsNoEmail
                ? services.AddSingleton<IBookingEmailService, NullBookingEmailService>()
                : services
                    .AddSingleton(emailOptions)
                    .AddSingleton<IBookingEmailService, PreferenceFormEmailService>();

    private static IServiceCollection AddBookingNoteService(
        this IServiceCollection services) =>
            IsNoNotes
                ? services.AddSingleton<IBookingNoteService, NullBookingNoteService>()
                : services.AddSingleton<IBookingNoteService, BookingNoteService>();

    private static bool IsDryRun =>
        Environment.GetCommandLineArgs().Contains("--dry-run");
    private static bool IsNoEmail =>
        IsDryRun ||
        Environment.GetCommandLineArgs().Contains("--no-email");
    private static bool IsNoNotes =>
        IsDryRun ||
        Environment.GetCommandLineArgs().Contains("--no-notes");
}
