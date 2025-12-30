namespace Back.DTOs
{
    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int ProgressPercentage { get; set; }
        public bool AllQuizzesPassed { get; set; }
    }
}