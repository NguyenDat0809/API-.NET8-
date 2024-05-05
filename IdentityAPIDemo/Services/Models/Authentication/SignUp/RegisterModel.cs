using System.ComponentModel.DataAnnotations;

namespace Services.Models.Authentication.SignUp
{
    public class RegisterModel
    {
        [Required(ErrorMessage = " User name is required")]
        public  string? UserName { get; set; }

        [EmailAddress] //dùng anotation nào cũng dc
        //[DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password is not the same")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public List<string> Roles { get; set; }
    }
}
