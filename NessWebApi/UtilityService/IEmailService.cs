using NessWebApi.Models;

namespace NessWebApi.UtilityService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel);
    }
}
