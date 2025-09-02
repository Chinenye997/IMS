
namespace Application.Interface
{
    public interface IEmailService
    {
        // Sends an email with subject and body to the recipient
        Task SendEmailAsync(string toEmail, string subjust, string body);
    }
}
