using System.ComponentModel.DataAnnotations;

namespace DTOs.User
{
    public class VerifyOTPRequest
    {
        [Required]
        public string OTP { get; set; } = null!;
        public string Email { get; set; } = null!;

    }

}