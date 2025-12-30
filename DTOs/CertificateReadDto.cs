namespace Back.DTOs
{
    public class CertificateReadDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string DownloadUrl { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}