

using System.ComponentModel.DataAnnotations;
using Models.Questions;
using Models.User;


namespace DTOs.Form

{
    public class SubmitFormRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public SharedUser User { get; set; } = null!;

        [Required]
        public List<BaseQuestion> Questions { get; set; } = new List<BaseQuestion>();
        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

    }

}