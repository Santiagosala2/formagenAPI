

using System.ComponentModel.DataAnnotations;
using Models;

namespace DTOs
{
    public class CreateFormRequest
    {

        [Required]
        public string Name { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<BaseQuestion>? Questions { get; set; }

    }

    public class CreateFormResponse
    {

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<BaseQuestion> Questions { get; } = new List<BaseQuestion>();
    }

}