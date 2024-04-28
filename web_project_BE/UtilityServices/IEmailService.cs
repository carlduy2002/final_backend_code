using web_project_BE.Models;

namespace web_project_BE.UtilityServices
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel);
    }
}
