

using System.ComponentModel.DataAnnotations;
using DTOs.User;


namespace Models.Form
{
    public class ShareLink
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public List<ShareUser> Users { get; set; } = new List<ShareUser>();
        public List<dynamic> Responses { get; set; } = new List<dynamic>();
        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

    }

}