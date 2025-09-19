using TaskManagerAPI.Data;
using MailKit.Net.Smtp;
using MimeKit;

namespace TaskManagerAPI.Services.Notifications
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly TaskManagerDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(TaskManagerDbContext context , IConfiguration config , ILogger<EmailService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Task Manager", _config["Email:Smtp:From"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Email:Smtp:Host"], _config["Email:Smtp:Port"]));
            await client.AuthenticateAsync(_config["Email:Smtp:User"], _config["Email:Smtp:Pass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To}", to);
        }
    }
}
