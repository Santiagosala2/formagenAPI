

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextQuestion), typeDiscriminator: "Text")]
    [JsonDerivedType(typeof(DateQuestion), typeDiscriminator: "Date")]
    [JsonDerivedType(typeof(CheckboxQuestion), typeDiscriminator: "Checkbox")]
    [JsonDerivedType(typeof(RadioQuestion), typeDiscriminator: "Radio")]
    public class BaseQuestion
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;

        public string? Label { get; set; }

        public string? Placeholder { get; set; }

        public string? Description { get; set; }

        public bool Required { get; set; }


    }


    public class TextQuestion : BaseQuestion
    {
        public string? DefaultValue { get; set; }
        public bool? Long { get; set; }

        public string Type = "Text";
    }

    public class DateQuestion : BaseQuestion
    {
        public DateTime? DefaultValue { get; set; }

        public bool? DateRestriction { get; set; }

        public string? DateRestrictionRule { get; set; }
        public string Type = "Date";

    }

    public class CheckboxQuestion : BaseQuestion
    {
        public bool? DefaultValue { get; set; }
        public string Type = "Checkbox";

    }


    public class RadioQuestion : BaseQuestion
    {
        public string? DefaultValue { get; set; }
        public string Type = "Radio";

    }

}