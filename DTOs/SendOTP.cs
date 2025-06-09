using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class SendOTPRequest
    {
        [Required]
        public string UserEmail { get; set; } = null!;

    }

    public class SendOTPResponse
    {
        public string OTPsent { get; set; } = null!;

    }

}