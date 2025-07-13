

using System.ComponentModel.DataAnnotations;
using Models.Questions;

namespace Models.Form
{
    public class Form
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<BaseQuestion> Questions { get; set; } = new List<BaseQuestion>();

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

    }

}