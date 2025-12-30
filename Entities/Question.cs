using System.Collections.Generic;

namespace Back.Entities
{
    public enum QuestionType
    {
        MCQ = 0,   // Multiple Choice
        TF = 1,    // True / False
        MSQ = 2    // Subjective (Manual grading)
    }

    public class Question
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public string Text { get; set; }

        public QuestionType Type { get; set; }

        public ICollection<Answer> Answers { get; set; }

        public int Points { get; set; }
    }
}
