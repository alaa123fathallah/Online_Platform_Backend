public class QuizSubmitDto
{
    public List<QuizAnswerDto> Answers { get; set; }
}

public class QuizAnswerDto
{
    public int QuestionId { get; set; }
    public int? SelectedAnswerId { get; set; } // MCQ
    public string TextAnswer { get; set; }     // Subjective
}
