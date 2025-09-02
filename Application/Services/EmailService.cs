using Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;  // inject config. for SMTP settings
        }

        // send email using SMTP.
        public async Task SendEmailAsync(string toEmail, string subjust, string body)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:Username"]),
                Subject = subjust,
                Body = body,
                IsBodyHtml = true // Allows html for reset link
            };
            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage); // send async

            //throw new NotImplementedException();
        }
    }
}
