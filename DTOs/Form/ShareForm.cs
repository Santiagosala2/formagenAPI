

using System.ComponentModel.DataAnnotations;

using Models.User;

namespace DTOs.Form
{
    public class ShareFormRequest
    {
        [Required]
        public string FormId { get; set; } = null!;
        [Required]
        public List<SharedUser> Users { get; set; } = new List<SharedUser>();
    }


}