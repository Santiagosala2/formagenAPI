
using System.ComponentModel.DataAnnotations;

namespace DTOs.Admin
{
    public class UpdateAdminUser
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

    }
}