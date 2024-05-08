using MailKit.Net.Smtp;
using MimeKit;
using Services.Models;
using Services.Services.Interfaces;


namespace Services.Services.Implements
{
    public class EmailService : IEmailService
    {
        //Tổng quan SmtpClient(MimeMessagge(Message))
        //SmtpClient chịu trách nhiệm kết nối đến máy chủ SMTP -> gửi Mail
        //MimeMessage chứa thông tin người gửi, người nhận, chủ đề, nội dung,...
        //Message là class được tạo để có thể custom nội dung cần gửi, đến những ai, người gửi là ai

        private readonly EmailConfiguration _emailConfig;
        public EmailService(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }

        //MimeKit.MimeMessage là class chính chứa nội dung được truyền tải như text, CC, attachfile,..., người gửi, người nhận
        //Là class chứa cả thông tin Message và EmailConfiguration
        //Là class đại diện cho Mail luôn á, mọi thứ
        private MimeMessage CreateEmailMessage(Message message)
        {
            //tạo mới đối tượng để lưu trữ nội dung email theo chuẩn MIME
            var emailMessage = new MimeMessage();

            //Thiết lập địa chỉ email người gửi cho message.
            //ctor(<tên người dùng>, <cấu hình người gửi>)
            emailMessage.From.Add(new MailboxAddress(_emailConfig.SenderName, _emailConfig.From));

            //thêm toàn bộ danh sách địa chỉ email người nhận (message.To) từ đối tượng message vào danh sách To của emailMessage.
            emailMessage.To.AddRange(message.To);

            //thêm chủ đề
            emailMessage.Subject = message.Subject;

            //thêm nội dung
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = message.Content
            };
            //còn nhiều property khác của MiMeMessage để thêm nội dung

            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            //SmtpClient dc sử dụng để  kết nối đến máy chủ SMTP -> gửi mail
            using var client = new SmtpClient();
            try
            {
                //hàm kết nối đến máy chủ SMTP
                //constructor sử dụng là ctor(host, port, useSsl)
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);

                /*
                 OAuth2 là một giao thức ủy quyền phổ biến được sử dụng để cho phép các ứng dụng truy cập vào tài nguyên của người dùng mà không cần biết thông tin đăng nhập của họ.
                */

                client.AuthenticationMechanisms.Remove("XOAUTH2");
                // xác thực với máy chủ SMTP (đã đăng ký trên google account trước đó)
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                //gửi mail
                client.Send(mailMessage);
            }
            catch
            {
                //log or throw
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }
}


