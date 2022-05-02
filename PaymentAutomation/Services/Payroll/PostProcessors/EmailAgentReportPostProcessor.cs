using MailKit.Net.Smtp;
using MimeKit;
using PaymentAutomation.Models;
using System.Diagnostics;

namespace PaymentAutomation.Services.Payroll;

internal class EmailAgentReportPostProcessor : IPayrollPostProcessor
{
    private readonly EmailConfiguration emailSettings;
    private readonly string fromName;
    private readonly string fromEmail;
    private readonly string ccName;
    private readonly string ccEmail;

    public EmailAgentReportPostProcessor(
        EmailConfiguration emailSettings,
        string fromName,
        string fromEmail,
        string ccName,
        string ccEmail
    )
    {
        this.emailSettings = emailSettings;
        this.fromName = fromName;
        this.fromEmail = fromEmail;
        this.ccName = ccName;
        this.ccEmail = ccEmail;
    }

    public void Process(string filepath, DateOnly weekEndingDate, Agent? agent)
    {
        if (agent is null) throw new ArgumentNullException(nameof(agent));

        DisplayPreview(filepath, agent);

        if (!DoesUserRequestToProceed(agent))
        {
            Console.WriteLine("Skipping email");
            return;
        }

        SendEmail(filepath, weekEndingDate, agent);
        Console.WriteLine($"Email sent to {agent.FullName} at {agent.Settings.Email}");
    }

    private void SendEmail(string filepath, DateOnly weekEndingDate, Agent agent)
    {
        var mailMessage = CreateMailMessage(filepath, weekEndingDate, agent);

        using var smtpClient = new SmtpClient();
        smtpClient.Connect(emailSettings.Server, emailSettings.Port, true);
        smtpClient.Authenticate(emailSettings.Username, emailSettings.Password);
        smtpClient.Send(mailMessage);
        smtpClient.Disconnect(true);
    }

    private MimeMessage CreateMailMessage(string filepath, DateOnly weekEndingDate, Agent agent)
    {
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
        mailMessage.To.Add(new MailboxAddress(agent.FullName, agent.Settings.Email));
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
            FileName = Path.GetFileName(filepath)
        };

        mailMessage.Body = new Multipart("mixed")
        {
            textBody,
            attachment
        };

        return mailMessage;
    }

    private static void DisplayPreview(string filepath, Agent agent)
    {
        Console.WriteLine($"\nPreviewing report for {agent.FullName}.");
        var previewProcess = new Process();
        previewProcess.StartInfo.FileName = "explorer";
        previewProcess.StartInfo.Arguments = filepath;
        previewProcess.Start();
    }

    private static bool DoesUserRequestToProceed(Agent agent)
    {
        string response;
        do
        {
            Console.Write($"Email report to {agent.Settings.Email}? y/n (y): ");
            response = Console.ReadLine()!.ToLower();
        } while (
            response.Length > 0 &&
            !response.StartsWith("y") &&
            !response.StartsWith("n")
        );

        return !response.StartsWith("n");
    }
}
