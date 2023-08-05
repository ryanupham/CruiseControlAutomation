using CruiseControl.Models;
using CruiseControl.Services;
using CruiseControl.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CruiseControl.Extensions;
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddUserService(
        this IServiceCollection services) =>
            services.AddSingleton<IUserService, UserService>();

    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        EmailConfiguration emailConfiguration) =>
            services.AddSingleton<ISmtpClientFactory>(
                new SmtpClientFactory(emailConfiguration))
            .AddSingleton<IEmailService, EmailService>();

    public static IServiceCollection AddBookingNoteService(
        this IServiceCollection services) =>
            services.AddSingleton<IBookingNoteService, BookingNoteService>();

    public static IServiceCollection AddCruiseControlHttpClient(
        this IServiceCollection services,
        string baseUrl)
    {
        var cookies = GetCruiseControlCookies(baseUrl);

        services.AddHttpClient<CruiseControlClient>(client =>
        {
            client.DefaultRequestHeaders.Add("Cookie", GetCookiesString());
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("bearer", cookies.accessToken);
            client.BaseAddress = new Uri($"https://{baseUrl}");
        });

        return services;

        string GetCookiesString() =>
            $"{nameof(cookies.NSC_TMAS)}={cookies.NSC_TMAS};" +
            $"{nameof(cookies.username)}={cookies.username};" +
            $"{nameof(cookies.JSESSIONID)}={cookies.JSESSIONID};" +
            $"{nameof(cookies.accessToken)}={cookies.accessToken};" +
            $"{nameof(cookies.refreshToken)}={cookies.refreshToken};";
    }

    private static CruiseControlCookies GetCruiseControlCookies(string baseUrl)
    {
        var cookies = ChromeCookieManager.GetCookies(baseUrl);
        CruiseControlCookies ccCookies;

        try
        {
            ccCookies = CruiseControlCookies.FromCookies(cookies);
        }
        catch (ArgumentException e)
        {
            throw new Exception("Unable to load cookies. Please ensure you are logged in to Cruise Control in Chrome.", e);
        }

        return ccCookies;
    }
}
