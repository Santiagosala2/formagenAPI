
using System.ComponentModel.DataAnnotations;
using Models;

namespace DTOs
{
    public class UpdateUser
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

    }
}