

using System.ComponentModel.DataAnnotations;

namespace DTOs.User
{
    public class CreateUser
    {


        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

    }
}
