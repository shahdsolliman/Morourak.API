using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
using Morourak.Application.Exceptions;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

[ApiController]
[Route("api/test")]
public class TestEmailController : ControllerBase
{
    [HttpGet("send-email")]
    public async Task<IActionResult> SendTestEmail()
    {
        try
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("shahd.soliman2050@gmail.com", "dflc pqcg lqss dwqy"),
                EnableSsl = true
            };

            var mail = new MailMessage("shahd.soliman2050@gmail.com", "shahdmohammed1511@gmail.com")
            {
                Subject = "Test Email",
                Body = "Hello! This is a test."
            };

            await client.SendMailAsync(mail);

            return Ok(ApiResponseArabic.Success(null, "تم إرسال البريد الإلكتروني بنجاح!"));
        }
        catch (System.Exception ex)
        {
            throw new ValidationException(ex.Message);
        }
    }
}
