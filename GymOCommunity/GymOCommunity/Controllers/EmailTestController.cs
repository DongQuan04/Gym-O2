using Microsoft.AspNetCore.Mvc;

public class EmailTestController : Controller
{
    private readonly EmailSender _emailSender;

    public EmailTestController(EmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<IActionResult> SendTestEmail()
    {
        await _emailSender.SendEmailAsync("test@example.com", "Test Email", "This is a test email from ASP.NET Core.");
        return Content("Email sent successfully!");
    }
}
