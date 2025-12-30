namespace Back.DTOs
{
    public class QuizResultDto
    {
        public int Score { get; set; }
        public bool Passed { get; set; }
        public int PassingScore { get; set; }
    }
}