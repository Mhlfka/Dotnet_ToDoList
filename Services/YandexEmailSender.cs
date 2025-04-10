using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ToDoListApp.Services
{
    public class YandexEmailSender
    {
        private readonly string _smtpServer = "smtp.yandex.com";
        private readonly int _smtpPort = 587; // Try 465 if 587 fails
        private readonly string _fromEmail;
        private readonly string _password;
        private readonly ILogger<YandexEmailSender> _logger;

        public YandexEmailSender(IConfiguration configuration, ILogger<YandexEmailSender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fromEmail = configuration["Yandex:Email"];
            _password = configuration["Yandex:AppPassword"];

            if (string.IsNullOrEmpty(_fromEmail) || string.IsNullOrEmpty(_password))
            {
                _logger.LogError("Yandex email configuration is missing. Email: '{Email}', Password: '{Password}'", 
                    _fromEmail ?? "null", _password != null ? "[redacted]" : "null");
                throw new InvalidOperationException("Yandex email or app password not configured.");
            }

            _logger.LogInformation("Initialized YandexEmailSender with Email: {Email}", _fromEmail);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("", _fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using (var client = new SmtpClient())
                {
                    _logger.LogInformation("Connecting to {Server}:{Port}...", _smtpServer, _smtpPort);
                    await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                    _logger.LogInformation("Authenticating...");
                    await client.AuthenticateAsync(_fromEmail, _password);

                    _logger.LogInformation("Sending email to {ToEmail} with subject '{Subject}'...", toEmail, subject);
                    await client.SendAsync(message);

                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email: {Message}", ex.Message);
                throw;
            }
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            SendEmailAsync(toEmail, subject, body).GetAwaiter().GetResult();
        }
    }
}