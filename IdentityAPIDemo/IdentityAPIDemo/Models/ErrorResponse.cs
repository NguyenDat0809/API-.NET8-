namespace IdentityAPIDemo.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }

        public string Error { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
