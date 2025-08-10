

using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Session
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;

        public string OTP { get; set; } = null!;

        public bool Used { get; set; } = false;
        public DateTime UseUntil { get; set; }

        public DateTime ExpiresAt { get; set; }
        public bool IsAdmin { get; set; }
        public int Ttl { get; set; }
    }

}