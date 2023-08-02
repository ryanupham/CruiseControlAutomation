using CruiseControl.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace CruiseControl.Services;

public interface ISmtpClientFactory
{
    SmtpClient CreateClient();
    MimeMessage CreateBaseMessage();
}

public class SmtpClientFactory : ISmtpClientFactory
{
    private readonly EmailConfiguration emailConfiguration;

    public SmtpClientFactory(EmailConfiguration emailConfiguration) =>
        this.emailConfiguration = emailConfiguration;

    public SmtpClient CreateClient()
    {
        var smtpClient = new SmtpClient();
        smtpClient.Connect(
            emailConfiguration.Server, emailConfiguration.Port, true);
        smtpClient.Authenticate(
            emailConfiguration.Username, emailConfiguration.Password);
        return smtpClient;
    }

    public MimeMessage CreateBaseMessage() =>
        new()
        {
            Sender = MailboxAddress.Parse(emailConfiguration.Username),
            From = { MailboxAddress.Parse(emailConfiguration.Username) },
        };
}
