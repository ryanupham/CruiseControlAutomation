using CruiseControl.Services;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using PriorToTravelEmailSender.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace PriorToTravelEmailSender.Services;

internal interface IBookingService
{
    IAsyncEnumerable<Booking> GetBookingsWithContactEmailsAsync(
        IEnumerable<Booking> bookings);
    Task<Booking> GetBookingWithContactEmailAsync(Booking booking);
    Task<IReadOnlyCollection<Booking>> GetUpcomingBookingsWithoutEmailAsync();
}

internal class BookingService : IBookingService
{
    private readonly ILogger<BookingService> logger;
    private readonly BookingsServiceSettings settings;
    private readonly CruiseControlClient httpClient;

    public BookingService(
        ILogger<BookingService> logger,
        BookingsServiceSettings settings,
        CruiseControlClient httpClient)
    {
        this.logger = logger;
        this.settings = settings;
        this.httpClient = httpClient;
    }
    
    public async IAsyncEnumerable<Booking> GetBookingsWithContactEmailsAsync(
        IEnumerable<Booking> bookings)
    {
        foreach (var booking in bookings)
        {
            yield return await GetBookingWithContactEmailAsync(booking);
        }
    }

    public async Task<Booking> GetBookingWithContactEmailAsync(Booking booking)
    {
        var email = await GetContactEmailForBookingAsync(booking.Id);
        return booking with { ContactEmail = email };
    }

    private async Task<string> GetContactEmailForBookingAsync(long bookingId)
    {
        var bookingDetails = await GetBookingDetailsAsync(bookingId);
        return bookingDetails.tripSummarySectionDto?.tripSummary?.emailContact?.address
            ?? throw new Exception($"No contact email found for booking {bookingId}");
    }

    private async Task<dynamic> GetBookingDetailsAsync(long bookingId)
    {
        logger.LogInformation("Fetching booking details for {bookingId}", bookingId);

        var urls = new[]
        {
            $"booking-api/booking/review/{bookingId}?fromLocation=as",
            $"land-api/land/sections/{bookingId}?fromLocation=land"
        };
        foreach (var url in urls)
        {
            var (success, bookingDetails) = await TryGetBookingReviewDetailsByUrl(url);
            if (success) return bookingDetails!;
        }

        throw new Exception($"Failed to fetch booking details for {bookingId}");
    }
    
    private async Task<(bool success, dynamic? bookingReviewDetails)>
        TryGetBookingReviewDetailsByUrl(string url)
    {
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return (false, null);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        return (result is not null, result);
    }

    public async Task<IReadOnlyCollection<Booking>> GetUpcomingBookingsWithoutEmailAsync()
    {
        var response = await httpClient.GetAsync(
            $"mycc/AgentBookings/username/{settings.Username}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Unable to retrieve bookings, status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return ParseBookingsFromHtml(content).ToList();
    }

    private IEnumerable<Booking> ParseBookingsFromHtml(string html)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var tableRows = htmlDocument.DocumentNode
            .QuerySelectorAll("#divMain > div.aligntop.width100pct > table > tr:nth-child(2) > td.middle > table > tr:nth-child(3) > td > table > tr")
            .Skip(1);
        return tableRows.Select(GetBookingFromTableRow);
    }

    private Booking GetBookingFromTableRow(HtmlNode row)
    {
        return new Booking(GetId(), GetDepartureDate(), GetFullName(), "");

        long GetId()
        {
            var idElementText = row
                .QuerySelector("td > a > span")
                .GetDirectInnerText().Trim();
            return long.Parse(idElementText);
        }

        string GetFullName() =>
            row
                .QuerySelector(":nth-child(4) > a > span")
                .GetDirectInnerText().Trim();

        DateOnly GetDepartureDate()
        {
            var departureDateElement = row.QuerySelector(":nth-child(8)");
            return DateOnly.Parse(
                departureDateElement.GetDirectInnerText().Trim());
        }
    }
}
