using MimeKit;

namespace CruiseControl.Services;
public interface IEmailService
{
    Task SendEmailAsync(MimeMessage message);
    MimeMessage GetBaseMessage();
}

public class EmailService : IEmailService
{
    private readonly ISmtpClientFactory smtpClientFactory;

    public EmailService(ISmtpClientFactory smtpClientFactory) =>
        this.smtpClientFactory = smtpClientFactory;

    public async Task SendEmailAsync(MimeMessage message)
    {
        using var smtpClient = smtpClientFactory.CreateClient();
        var result = await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }

    public MimeMessage GetBaseMessage() =>
        smtpClientFactory.CreateBaseMessage();
}
