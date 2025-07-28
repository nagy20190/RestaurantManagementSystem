using DeliveryManagementSystem.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly EmailServices _emailServices;

        public TestController(EmailServices emailServices)
        {
            _emailServices = emailServices;
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                string emailSubject = "Contact Confirmation";
                string username = "Mostafa Nagy";
                string emailMessage = "Dear " + username + "\n" +
                    "We received your message. Thank you for contacting us.\n" +
                    "Our team will contact you very soon.\n" +
                    "Best Regards\n\n" +
                    "Your Message:\n" + "Your Order Confirmation.";
                string email = "mostafanagy679@gmail.com";

                await _emailServices.SendEmail(emailSubject, email, username, emailMessage);

                return Ok("Email Sent Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
        }




    }
}
