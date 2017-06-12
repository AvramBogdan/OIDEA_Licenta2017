using System.ComponentModel.DataAnnotations;

namespace OTP.ViewModels.User
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Required field")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Required field")]
        public string Password { get; set; }
    }
}
