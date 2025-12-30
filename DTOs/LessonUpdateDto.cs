namespace Back.DTOs
{
    public class LessonUpdateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public int Order { get; set; }
        public int EstimatedDuration { get; set; }
    }
}