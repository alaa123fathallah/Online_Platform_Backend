using System.Collections.Generic;

namespace Back.DTOs
{
    public class QuestionCreateDto
    {
        // Text shown to the student
        public string QuestionText { get; set; }

        // Must be: "MCQ", "TF", or "MSQ"
        public string QuestionType { get; set; }

        // Required for MCQ and TF, empty for MSQ
        public List<AnswerCreateDto> Answers { get; set; }
    }
}
