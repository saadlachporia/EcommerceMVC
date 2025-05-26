public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendAccountCreationConfirmationAsync(string toEmail, string userName);
    Task SendPasswordResetAsync(string toEmail, string resetLink);
}
