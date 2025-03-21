using Microsoft.AspNetCore.Mvc;

public class TestController : Controller
{
    private readonly EmailSender _emailSender;

    public TestController(EmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<IActionResult> SendTestEmail()
    {
        await _emailSender.SendEmailAsync("test@example.com", "Test Email", "This is a test email from ASP.NET Core.");
        return Content("Email sent successfully!");
    }
}
