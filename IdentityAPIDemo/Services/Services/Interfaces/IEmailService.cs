using Services.Models;

namespace Services.Services.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(Message message);
    }
}
