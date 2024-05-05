

using Services.Models;

namespace Services.Services
{
    public interface IEmailService
    {
       public void SendEmail(Message message);
    }
}
