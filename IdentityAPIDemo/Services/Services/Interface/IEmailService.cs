using Services.Models;

namespace Services.Services.Interface
{
    public interface IEmailService
    {
        public void SendEmail(Message message);
    }
}
