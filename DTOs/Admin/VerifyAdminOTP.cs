using System.ComponentModel.DataAnnotations;

namespace DTOs.Admin
{
    public class VerifyAdminOTPRequest
    {
        [Required]
        public string OTP { get; set; } = null!;
        public string Email { get; set; } = null!;

    }

}