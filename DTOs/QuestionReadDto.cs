using Back.DTOs;
using System.Collections.Generic;

namespace Back.DTOs
{
    public class QuestionReadDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public List<AnswerReadDto> Answers { get; set; }
    }
}