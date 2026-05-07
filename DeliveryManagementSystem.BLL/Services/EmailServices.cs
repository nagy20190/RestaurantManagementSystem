using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace DeliveryManagementSystem.BLL.Services
{
    public class EmailServices
    {
        private readonly string apiKey;
        private readonly string fromEmail;
        private readonly string senderName;

        public EmailServices(IConfiguration configuration)
        {
            apiKey = configuration["SendGrid:ApiKey"]!;
            fromEmail = configuration["SendGrid:FromEmail"]!;
            senderName = configuration["SendGrid:SenderName"]!;
        }
        public async Task SendEmail
            (string subject, string toEmail, string userName, string message)
        {
            var client = new SendGridClient(apiKey);
            var from =
                new EmailAddress(fromEmail, senderName);
            var to = new EmailAddress(toEmail, userName);
            var plainTextContent = message;
            var htmlContent = $"<strong>{message}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            // Check if it's successfully sent

            Console.WriteLine(response.StatusCode);
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string userName, string confirmationLink)
        {
            try
            {
                var subject = "Confirm Your Email Address";
                var htmlContent = $@"
            <html>
            <body>
                <h2>Welcome {userName}!</h2>
                <p>Please confirm your email address by clicking the link below:</p>
                <a href='{confirmationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    Confirm Email
                </a>
                <p>If the button doesn't work, copy and paste this link into your browser:</p>
                <p>{confirmationLink}</p>
                <p>This link will expire in 24 hours.</p>
            </body>
            </html>";

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, senderName);
                var to = new EmailAddress(email, userName);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
                var response = await client.SendEmailAsync(msg);
                var body = await response.Body.ReadAsStringAsync();

                // Optionally, check response.StatusCode for success/failure
                Console.WriteLine($"{response.StatusCode} ,, {body}");
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    // Log the failure or handle it as needed
                    Console.WriteLine($"Failed to send email: {response.StatusCode}");
                    return false;
                }
            }
            catch 
            {
                // Optionally log the error
                throw;
            }
            return true;
        }



    }
}

