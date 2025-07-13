using System.ComponentModel.DataAnnotations;

namespace DTOs.Admin
{
    public class SendAdminOTPRequest
    {
        [Required]
        public string Email { get; set; } = null!;

    }

    public class SendAdminOTPResponse
    {
        public string OTPsent { get; set; } = null!;

    }

}