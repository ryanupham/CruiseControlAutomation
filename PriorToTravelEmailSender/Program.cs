using CruiseControl.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PriorToTravelEmailSender.Extensions;
using PriorToTravelEmailSender.Models;
using PriorToTravelEmailSender.Services;
using System.Diagnostics;

namespace PriorToTravelEmailSender;
internal class Program
{
    public static async Task Main()
    {
        if (IsAppRunning())
        {
            Console.WriteLine("App is already running. Exiting in 10 seconds...");
            await Task.Delay(10000);
            return;
        }

        while (true)
        {
            try
            {
                var appSettings =
                    UserFileHelper.LoadSettingsFile<AppSettings>("appsettings.json");

                using var serviceProvider = CreateServiceProvider(appSettings);
                var upcomingBookingsEmailProcessor = serviceProvider
                    .GetRequiredService<IUpcomingBookingsEmailProcessor>();

                await upcomingBookingsEmailProcessor.Run(appSettings.DaysUntilTravel);

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n\n{e}");
                Console.WriteLine($"\n\nError: {e.Message}");
            }

            Console.WriteLine("\n\nPress any key to try again...");
            Console.ReadKey();
        }
    }

    private static ServiceProvider CreateServiceProvider(AppSettings appSettings) =>
        new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .AddUpcomingBookingsEmailProcessor(appSettings)
            .BuildServiceProvider();

    private static bool IsAppRunning()
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        return Process.GetProcessesByName(processName).Length > 1;
    }
}
