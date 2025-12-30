namespace Back.DTOs
{
    public class QuizReadDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? LessonId { get; set; }
        public string Title { get; set; }
        public int PassingScore { get; set; }
        public int TimeLimit { get; set; }
    }
}