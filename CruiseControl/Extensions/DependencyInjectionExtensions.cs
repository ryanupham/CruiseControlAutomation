using CruiseControl.Models;
using CruiseControl.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace CruiseControl.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCruiseControlHttpClient(
        this IServiceCollection services,
        string baseUrl)
    {
        var cookies = GetCruiseControlCookies(baseUrl);

        services.AddHttpClient<HttpClient>(client =>
        {
            client.DefaultRequestHeaders.Add("Cookie", $"{nameof(cookies.NSC_TMAS)}={cookies.NSC_TMAS}");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", cookies.accessToken);
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
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
