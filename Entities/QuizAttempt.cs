public class QuizAttempt
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int UserId { get; set; }

    public int? Score { get; set; }
    public bool IsPassed { get; set; }
    public bool IsGraded { get; set; }

    public ICollection<StudentAnswer> StudentAnswers { get; set; }
}
