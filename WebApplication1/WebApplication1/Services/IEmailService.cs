namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task GuiEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    }
}
