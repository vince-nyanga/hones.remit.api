using System.Net.Mail;

namespace Hones.Remit.Api.Services;

internal sealed class LocalSmtpEmailService : IEmailService
{
    private const string From = "noreply@honesremit.com";
    
    private readonly SmtpClient _smtpClient;

    public LocalSmtpEmailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MailMessage(From, to, subject, body);
        await _smtpClient.SendMailAsync(message);
    }
}