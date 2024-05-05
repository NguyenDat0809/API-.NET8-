using MimeKit;

namespace Services.Models
{
    /// <summary>
    /// Class đại diện cho toàn bộ mail của người dùng, từ người nhận -> người gửi, 
    /// bao gồm các thông tin: (To) Danh sách người nhận, (Subject) Chủ đề, (Content) Nội dung, Người gửi
    /// </summary>
    public class Message
    {
        public string? From { get; set; }
        public List<MailboxAddress> To { get; set; }
        public string? Subject  { get; set; }
        public string? Content { get; set; }
        

        public Message(IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            /*
            MimeKit.MailboxAddress là 1 class của thư viện MimeKit, xây dụng lên 1 intance người dùng Mail, chứa thông tin Mail của người dùng đó
            Class này có nhiều thuộc tính và có nhiều constructor NHƯNG BASIC THÌ DÙNG MailboxAddress(name, address)
            Đầy đủ thuộc tính của MailboxAddress
            - Address
            - Domain
            - Encoding
            - IdnMapping
            - IsInternational
            - LocalPart
            - Name
            - Route
             */
            //name tương đương với lại tên người dùng
            //address tương đương với địa chỉ người dùng
            To.AddRange(to.Select(x => new MailboxAddress("email", x)));
            Subject = subject;
            Content = content;
        }
    }
}
