namespace Back.DTOs
{
    public class CourseCreateDto
    {
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Category { get; set; }
        public string Difficulty { get; set; }
        public string Thumbnail { get; set; }
    }
}