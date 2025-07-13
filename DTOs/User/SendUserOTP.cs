using System.ComponentModel.DataAnnotations;

namespace DTOs.User
{
    public class SendUserOTPRequest
    {
        [Required]
        public string Email { get; set; } = null!;

    }

    public class SendUserOTPResponse
    {
        public string OTPsent { get; set; } = null!;

    }

}