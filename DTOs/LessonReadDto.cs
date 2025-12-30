namespace Back.DTOs
{
    public class LessonReadDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public int Order { get; set; }
        public int EstimatedDuration { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}