using PriorToTravelEmailSender.Models;

namespace PriorToTravelEmailSender.Services;
internal interface IBookingEmailService
{
    Task SendEmailAsync(Booking booking);
}
