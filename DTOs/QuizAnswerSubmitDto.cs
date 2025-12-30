namespace Back.DTOs
{
    public class QuizAnswerSubmitDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; }
    }
}