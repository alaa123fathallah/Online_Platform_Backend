namespace Back.DTOs
{
    public class QuizCreateDto
    {
        public int CourseId { get; set; }
        public int? LessonId { get; set; } // optional
        public string Title { get; set; }
        public int PassingScore { get; set; }
        public int TimeLimit { get; set; }
    }
}