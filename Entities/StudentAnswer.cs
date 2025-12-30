public class StudentAnswer
{
    public int Id { get; set; }
    public int QuizAttemptId { get; set; }
    public int QuestionId { get; set; }

    public int? SelectedAnswerId { get; set; } 
    public string TextAnswer { get; set; }     

    public int? PointsAwarded { get; set; }    
}
