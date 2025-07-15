

using System.ComponentModel.DataAnnotations;
using Models.User;


namespace Models.Form
{
    public class FormResponse
    {
        [Required]
        public string FormId { get; set; } = null!;
        public SharedUser User { get; set; } = null!;
        public dynamic Response { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

    }

}