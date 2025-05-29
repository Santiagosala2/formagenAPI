

using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Form
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        [Required]
        public List<BaseQuestion> Questions { get; set; } = null!;

    }

    public class FormRequest
    {

        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        [Required]
        public List<BaseQuestion> Questions { get; set; } = null!;

    }

    public class FormResponse
    {

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;

    }

}