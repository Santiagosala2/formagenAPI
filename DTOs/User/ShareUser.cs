
using System.ComponentModel.DataAnnotations;

namespace DTOs.User
{
    public class ShareUser
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

    }
}
