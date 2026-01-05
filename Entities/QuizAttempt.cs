using System;

namespace Back.Entities
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public int UserId { get; set; }

        public int Score { get; set; }

        public bool Passed { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        public Quiz Quiz { get; set; }
        public User User { get; set; }
    }
}
