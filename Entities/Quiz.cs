using Back.Entities;

namespace Back.Entities
{
    public class Quiz
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int? LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public int PassingScore { get; set; }
        public int TimeLimit { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}
