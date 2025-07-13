

using System.ComponentModel.DataAnnotations;

namespace Models.User
{
    public class UserSession
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        public string OTP { get; set; } = null!;

        public bool Used { get; set; } = false;
        public DateTime UseUntil { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

}