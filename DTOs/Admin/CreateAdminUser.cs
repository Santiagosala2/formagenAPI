

using System.ComponentModel.DataAnnotations;

namespace DTOs.Admin
{
    public class CreateAdminUserRequest
    {

        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

    }
}
