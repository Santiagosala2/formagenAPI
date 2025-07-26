
using System.ComponentModel.DataAnnotations;

namespace DTOs.Admin
{
    public class SessionResponse
    {
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public bool IsAdmin { get; set; } = false;
    }

}