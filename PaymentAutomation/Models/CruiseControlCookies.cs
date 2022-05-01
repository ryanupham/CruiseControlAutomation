using System.Net;

namespace PaymentAutomation.Models;

#pragma warning disable IDE1006 // Naming Styles
public record CruiseControlCookies(string JSESSIONID, string NSC_TMAS, string accessToken, string refreshToken, string username)
#pragma warning restore IDE1006 // Naming Styles
{
    public static CruiseControlCookies FromCookies(ICollection<Cookie> cookies)
    {
        try
        {
            return new CruiseControlCookies(
                GetCookieFromList(nameof(JSESSIONID)),
                GetCookieFromList(nameof(NSC_TMAS)),
                GetCookieFromList(nameof(accessToken)),
                GetCookieFromList(nameof(refreshToken)),
                GetCookieFromList(nameof(username))
            );
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("Missing cookies", nameof(cookies));
        }

        string GetCookieFromList(string cookieName) =>
            cookies.First(c => c.Name == cookieName).Value;
    }
}
