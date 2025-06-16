

using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class AdminUser
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

        public bool IsOwner { get; set; } = false;
    }

}