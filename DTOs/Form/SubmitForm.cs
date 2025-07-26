

using System.ComponentModel.DataAnnotations;
using Models.Questions;
using Models;
using DTOs.Admin;


namespace DTOs.Form

{
    public class SubmitFormRequest
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public SessionResponse User { get; set; } = null!;

        [Required]
        public List<BaseQuestion> Questions { get; set; } = new List<BaseQuestion>();

    }

}