

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

}