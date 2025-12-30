using System.Collections.Generic;

namespace Back.Entities
{
    public class Answer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        // Navigation property
        public Question Question { get; set; }
    }
}
