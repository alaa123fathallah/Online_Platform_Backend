public class EnrollmentReadDto
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }

    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int ProgressPercent { get; set; }
}
