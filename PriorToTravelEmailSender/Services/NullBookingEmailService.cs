using Microsoft.Extensions.Logging;
using PriorToTravelEmailSender.Models;

namespace PriorToTravelEmailSender.Services;
internal class NullBookingEmailService : IBookingEmailService
{
    private readonly ILogger<NullBookingEmailService> logger;

    public NullBookingEmailService(ILogger<NullBookingEmailService> logger) =>
        this.logger = logger;

    public Task SendEmailAsync(Booking booking)
    {
        logger.LogInformation(
            "Would have sent email to {FullName} ({ContactEmail})",
            booking.FullName,
            booking.ContactEmail);

        return Task.CompletedTask;
    }
}
