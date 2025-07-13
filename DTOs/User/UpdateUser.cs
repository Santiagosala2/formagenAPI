
using System.ComponentModel.DataAnnotations;

namespace DTOs.User
{
    public class UpdateUser
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

    }
}