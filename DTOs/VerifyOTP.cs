using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class VerifyOTPRequest
    {
        [Required]
        public string OTP { get; set; } = null!;
        public string Email { get; set; } = null!;

    }

}