public class QuizGradeDto
{
    public List<QuestionGradeDto> Grades { get; set; }
}

public class QuestionGradeDto
{
    public int StudentAnswerId { get; set; }
    public int PointsAwarded { get; set; }
}
