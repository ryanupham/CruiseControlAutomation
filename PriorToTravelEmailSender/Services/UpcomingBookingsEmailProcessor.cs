using CruiseControl.Models;
using CruiseControl.Services;
using PriorToTravelEmailSender.Extensions;
using PriorToTravelEmailSender.Models;

namespace PriorToTravelEmailSender.Services;
internal interface IUpcomingBookingsEmailProcessor
{
    Task Run(int daysUntilTravel);
}

internal class UpcomingBookingsEmailProcessor : IUpcomingBookingsEmailProcessor
{
    private readonly IBookingService bookingService;
    private readonly IBookingEmailService emailService;
    private readonly IUserService userService;
    private readonly IBookingNoteService noteService;
    private readonly IApplicationExecutionHistoryService executionHistoryService;

    public UpcomingBookingsEmailProcessor(
        IBookingService bookingService,
        IBookingEmailService bookingEmailService,
        IUserService userService,
        IBookingNoteService bookingNoteService,
        IApplicationExecutionHistoryService executionHistoryService)
    {
        this.bookingService = bookingService;
        this.emailService = bookingEmailService;
        this.userService = userService;
        this.noteService = bookingNoteService;
        this.executionHistoryService = executionHistoryService;
    }

    public async Task Run(int daysUntilTravel)
    {
        var bookings = await GetTargetBookings(daysUntilTravel);

        await foreach (var booking in bookingService
            .GetBookingsWithContactEmailsAsync(bookings))
        {
            await emailService.SendEmailAsync(booking);
            var createNoteModel = await CreateAddNoteModel(booking);
            await noteService.AddNote(booking.Id, createNoteModel);
            executionHistoryService.AddProcessedBookingId(booking.Id);
        }

        UpdateLastExecutionDate();
    }

    private async Task<IEnumerable<Booking>> GetTargetBookings(int daysUntilTravel)
    {
        var processedBookingIds = executionHistoryService.GetProcessedBookingIds();
        var bookings = await bookingService.GetUpcomingBookingsWithoutEmailAsync();
        if (!bookings.Any())
        {
            throw new Exception("No bookings found. Please ensure you're logged in.");
        }

        var startDate = executionHistoryService.GetLastExecutionDate()
            .AddDays(1 + daysUntilTravel);
        var endDate = DateTime.Now.ToDateOnly()
            .AddDays(daysUntilTravel);
        return bookings
            .Where(b => !processedBookingIds.Contains(b.Id))
            .Where(b => b.DepartureDate.IsBetweenInclusive(startDate, endDate));
    }

    private void UpdateLastExecutionDate() =>
        executionHistoryService.SetLastExecutionDate(DateTime.Now.ToDateOnly());

    private async Task<BookingNoteCreate> CreateAddNoteModel(Booking booking)
    {
        var userProfile = await GetUserProfile();
        return new(
            $"Preference form email sent. [Emailed to: {booking.FullName} &lt;{booking.ContactEmail}&gt;].",
            false,
            DateTime.UtcNow,
            true,
            false,
            "G",
            true,
            false,
            userProfile.FullName,
            userProfile.Username
        );
    }

    private Lazy<Task<UserProfile>>? userProfile;
    private async Task<UserProfile> GetUserProfile()
    {
        userProfile ??= new Lazy<Task<UserProfile>>(userService.GetUserProfile);
        return await userProfile.Value;
    }
}
