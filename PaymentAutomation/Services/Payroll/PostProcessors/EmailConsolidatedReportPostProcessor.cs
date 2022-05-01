using MailKit.Net.Smtp;
using MimeKit;
using PaymentAutomation.Models;

namespace PaymentAutomation.Services.Payroll;

internal class EmailConsolidatedReportPostProcessor : IPayrollPostProcessor
{
    private readonly EmailConfiguration emailSettings;
    private readonly string fromName;
    private readonly string fromEmail;
    private readonly string toName;
    private readonly string toEmail;
    private readonly string ccName;
    private readonly string ccEmail;

    public EmailConsolidatedReportPostProcessor(
        EmailConfiguration emailSettings,
        string fromName,
        string fromEmail,
        string toName,
        string toEmail,
        string ccName = "",
        string ccEmail = ""
    )
    {
        this.emailSettings = emailSettings;
        this.fromName = fromName;
        this.fromEmail = fromEmail;
        this.toName = toName;
        this.toEmail = toEmail;
        this.ccName = ccName;
        this.ccEmail = ccEmail;
    }

    public void Process(string filepath, DateOnly weekEndingDate, Agent? agent)
    {
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
        mailMessage.To.Add(new MailboxAddress(toName, toEmail));
        if (!string.IsNullOrEmpty(ccName) && !string.IsNullOrEmpty(ccEmail))
        {
            mailMessage.Cc.Add(new MailboxAddress(ccName, ccEmail));
        }
        mailMessage.Subject = $"Week Ending {weekEndingDate}";

        var textBody = new TextPart("plain")
        {
            Text = ""
        };

        var attachment = new MimePart("application", "pdf")
        {
            Content = new MimeContent(File.OpenRead(filepath)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = Path.GetFileName(filepath),
        };

        mailMessage.Body = new Multipart("mixed")
        {
            textBody,
            attachment,
        };

        using var smtpClient = new SmtpClient();
        smtpClient.Connect(emailSettings.Server, emailSettings.Port, true);
        smtpClient.Authenticate(emailSettings.Username, emailSettings.Password);
        smtpClient.Send(mailMessage);
        smtpClient.Disconnect(true);
    }
}
