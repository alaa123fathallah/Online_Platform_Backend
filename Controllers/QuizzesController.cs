using Back.Data;
using Back.DTOs;
using Back.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizzesController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // CREATE QUIZ
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto dto)
        {
            var quiz = new Quiz
            {
                Title = dto.Title,
                CourseId = dto.CourseId,
                LessonId = dto.LessonId,
                PassingScore = dto.PassingScore,
                TimeLimit = dto.TimeLimit
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return Ok(new { id = quiz.Id });
        }

        // ===============================
        // UPDATE QUIZ
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizCreateDto dto)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            quiz.Title = dto.Title;
            quiz.CourseId = dto.CourseId;
            quiz.LessonId = dto.LessonId;
            quiz.PassingScore = dto.PassingScore;
            quiz.TimeLimit = dto.TimeLimit;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ===============================
        // DELETE QUIZ
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ===============================
        // ADD QUESTION TO QUIZ
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpPost("{quizId}/questions")]
        public async Task<IActionResult> AddQuestion(int quizId, [FromBody] QuestionCreateDto dto)
        {
            var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == quizId);
            if (!quizExists)
                return NotFound("Quiz not found");

            var question = new Question
            {
                QuizId = quizId,
                Text = dto.QuestionText,
                Type = Enum.Parse<QuestionType>(dto.QuestionType),
                Points = 1
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            if (dto.Answers != null && dto.Answers.Any())
            {
                foreach (var a in dto.Answers)
                {
                    _context.Answers.Add(new Answer
                    {
                        QuestionId = question.Id,
                        Text = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    });
                }

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // ===============================
        // GET QUESTIONS BY QUIZ
        // ===============================
        [Authorize]
        [HttpGet("{quizId}/questions")]
        public async Task<IActionResult> GetQuizQuestions(int quizId)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Answers)
                .Select(q => new QuestionReadDto
                {
                    Id = q.Id,
                    QuestionText = q.Text,
                    QuestionType = q.Type.ToString(),
                    Answers = q.Answers.Select(a => new AnswerReadDto
                    {
                        Id = a.Id,
                        AnswerText = a.Text
                    }).ToList()
                })
                .ToListAsync();

            return Ok(questions);
        }

        // ===============================
        // GET QUIZZES BY COURSE
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetQuizzesByCourse(int courseId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .Select(q => new QuizReadDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    CourseId = q.CourseId,
                    LessonId = q.LessonId,
                    PassingScore = q.PassingScore,
                    TimeLimit = q.TimeLimit
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        // ===============================
        // GET QUIZ BY ID
        // ===============================
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            return Ok(new QuizReadDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                CourseId = quiz.CourseId,
                LessonId = quiz.LessonId,
                PassingScore = quiz.PassingScore,
                TimeLimit = quiz.TimeLimit
            });
        }

        // ===============================
        // SUBMIT QUIZ
        // ===============================
        [Authorize(Roles = "Student")]
        [HttpPost("{quizId}/submit")]
        public async Task<IActionResult> SubmitQuiz(int quizId, [FromBody] QuizSubmitDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return NotFound();

            int score = 0;

            foreach (var submitted in dto.Answers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == submitted.QuestionId);
                if (question == null) continue;

                var correct = question.Answers.FirstOrDefault(a =>
                    a.Id == submitted.SelectedAnswerId && a.IsCorrect);

                if (correct != null)
                    score++;
            }

            var attempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Score = score
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return Ok(new QuizResultDto
            {
                Score = score,
                Passed = score >= quiz.PassingScore
            });
        }

        // ===============================
        // GET QUIZ BY LESSON (STUDENT)
        // ===============================
        [Authorize(Roles = "Student")]
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetQuizByLesson(int lessonId)
        {
            var quiz = await _context.Quizzes
                .Where(q => q.LessonId == lessonId)
                .Select(q => new QuizReadDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    CourseId = q.CourseId,
                    LessonId = q.LessonId,
                    PassingScore = q.PassingScore,
                    TimeLimit = q.TimeLimit
                })
                .FirstOrDefaultAsync();

            if (quiz == null)
                return NotFound();

            return Ok(quiz);
        }

        // ======================================================
        // INSTRUCTOR: GET QUIZ SUBMISSIONS
        // ======================================================
        [Authorize(Roles = "Instructor")]
        [HttpGet("{quizId}/submissions")]
        public async Task<IActionResult> GetQuizSubmissions(int quizId)
        {
            var submissions = await _context.QuizAttempts
                .Where(a => a.QuizId == quizId)
                .Include(a => a.User)
                .Select(a => new
                {
                    AttemptId = a.Id,
                    UserId = a.User.Id,
                    FullName = a.User.FullName,
                    Email = a.User.Email,
                    Score = a.Score
                })
                .ToListAsync();

            return Ok(submissions);
        }


        // ======================================================
        // 🆕 INSTRUCTOR: GET QUIZ ATTEMPT DETAILS
        // ======================================================
        [Authorize(Roles = "Instructor")]
        [HttpGet("attempt/{attemptId}")]
        public async Task<IActionResult> GetQuizAttempt(int attemptId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
                return NotFound();

            return Ok(new
            {
                AttemptId = attempt.Id,
                Score = attempt.Score,
                Questions = attempt.Quiz.Questions.Select(q => new
                {
                    QuestionText = q.Text,
                    Answers = q.Answers.Select(a => new
                    {
                        AnswerText = a.Text,
                        IsCorrect = a.IsCorrect
                    })
                })
            });
        }

        // ======================================================
        // 🆕 INSTRUCTOR: GRADE / OVERRIDE SCORE
        // ======================================================
        [Authorize(Roles = "Instructor")]
        [HttpPut("attempt/{attemptId}/grade")]
        public async Task<IActionResult> GradeQuizAttempt(int attemptId, [FromBody] int score)
        {
            var attempt = await _context.QuizAttempts.FindAsync(attemptId);
            if (attempt == null)
                return NotFound();

            attempt.Score = score;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
