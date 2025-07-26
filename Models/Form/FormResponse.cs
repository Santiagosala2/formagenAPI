

using System.ComponentModel.DataAnnotations;
using Models.Questions;
using Models.User;


namespace Models.Form
{
    public class FormResponse
    {
        public required string Id { get; set; }

        public required string FormId { get; set; }
        public required SharedUser User { get; set; }
        public required List<BaseQuestion> Questions { get; set; } = new List<BaseQuestion>();

        public required DateTime Created { get; set; }

    }

}