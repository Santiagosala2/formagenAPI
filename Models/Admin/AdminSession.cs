

using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class AdminSession
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string UserEmail { get; set; } = null!;
        public string OTP { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
    }

}