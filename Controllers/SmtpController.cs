using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Services.Notifications;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmtpController : Controller
    {
        private readonly IEmailService _emailService;

        public SmtpController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("send")]
        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "gbemilanre@outlook.com",  // Replace with your Gmail
                    "Urgent Mail",
                    "If you get this Email , It means you have a sibling In OAU who is going through a lot ."
                );

                return Ok("Test email sent successfully! Check your inbox.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }
    }
}
