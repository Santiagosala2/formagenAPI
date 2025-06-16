

using System.ComponentModel.DataAnnotations;
using Models;

namespace DTOs
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
