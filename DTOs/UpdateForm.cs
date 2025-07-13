

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Models.Questions;

namespace DTOs
{
    public class UpdateFormRequest
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }

        [Required]
        public List<BaseQuestion> Questions { get; set; } = null!;

        [JsonIgnore]
        public DateTime Created { get; set; }

        [JsonIgnore]
        public DateTime LastUpdated { get; set; }

    }

    public class UpdateFormResponse
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<BaseQuestion> Questions { get; set; } = null!;
        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

    }

}