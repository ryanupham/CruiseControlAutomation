using CruiseControl.Services;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using PriorToTravelEmailSender.Models;

namespace PriorToTravelEmailSender.Services;

internal class PreferenceFormEmailService : IBookingEmailService
{
    private readonly EmailOptions emailOptions;
    private readonly ILogger<PreferenceFormEmailService> logger;
    private readonly IEmailService emailService;
    
    public PreferenceFormEmailService(
        EmailOptions emailOptions,
        ILogger<PreferenceFormEmailService> logger,
        IEmailService emailService)
    {
        this.emailOptions = emailOptions;
        this.logger = logger;
        this.emailService = emailService;
    }

    public async Task SendEmailAsync(Booking booking)
    {
        var message = GetEmailMessage(booking);
        logger.LogInformation(
            "Sending email to {FullName} ({ContactEmail})",
            booking.FullName,
            booking.ContactEmail);
        await emailService.SendEmailAsync(message);
    }

    private MimeMessage GetEmailMessage(Booking booking)
    {
        var message = emailService.GetBaseMessage();
        if (emailOptions.Cc.Any())
        {
            message.Cc.AddRange(emailOptions.Cc.Select(MailboxAddress.Parse));
        }
        if (emailOptions.Bcc.Any())
        {
            message.Bcc.AddRange(emailOptions.Bcc.Select(MailboxAddress.Parse));
        }

        message.To.Add(MailboxAddress.Parse(booking.ContactEmail));
        message.Subject = "Please complete your document preference form";
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = GetEmailBody(booking),
            ContentTransferEncoding = ContentEncoding.QuotedPrintable,
        };

        return message;
    }

    private string GetEmailBody(Booking booking) =>
        emailOptions.Template.Replace("{{FullName}}", booking.FullName);
}
