

using System.ComponentModel.DataAnnotations;

namespace Models.Admin
{
    public class BaseUser
    {
        public required string Id { get; set; }

        public required string Email { get; set; }
        public required string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }
    }

}