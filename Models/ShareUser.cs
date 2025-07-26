
using System.ComponentModel.DataAnnotations;

namespace Models.User
{
    public class SharedUser
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
