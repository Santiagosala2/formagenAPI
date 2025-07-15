

using System.ComponentModel.DataAnnotations;
using Models.Questions;

namespace DTOs.Form
{
    public class RemoveAccessFormRequest
    {

        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;


    }

    public class RemoveAccessFormResponse
    {

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<BaseQuestion> Questions { get; } = new List<BaseQuestion>();
    }

}