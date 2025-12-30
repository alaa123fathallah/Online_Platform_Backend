using Back.Data;
using Back.DTOs;
using Back.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        // ============================
        // GET quizzes by course
        // ============================
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetQuizzesByCourse(int courseId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .Select(q => new QuizReadDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    PassingScore = q.PassingScore,
                    TimeLimit = q.TimeLimit,
                    CourseId = q.CourseId,
                    LessonId = q.LessonId
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        // ============================
        // CREATE quiz
        // ============================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz(QuizCreateDto dto)
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

        // ============================
        // ADD QUESTION TO QUIZ (FINAL FIX)
        // ============================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("{quizId}/questions")]
        public async Task<IActionResult> AddQuestionToQuiz(
            int quizId,
            [FromBody] QuestionCreateDto dto)
        {
            try
            {
                // 1️⃣ Check quiz exists
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                    return NotFound("Quiz not found");

                // 2️⃣ Parse QuestionType enum
                if (!Enum.TryParse<QuestionType>(dto.QuestionType, true, out var parsedType))
                    return BadRequest("Invalid QuestionType");

                // 3️⃣ Create question (Points MUST be set)
                var question = new Question
                {
                    Text = dto.QuestionText,
                    Type = parsedType,
                    QuizId = quizId,
                    Points = 1 // IMPORTANT: prevents DB constraint failure
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                // 4️⃣ Add answers (validate text to avoid DB crash)
                if (dto.Answers != null && dto.Answers.Any())
                {
                    foreach (var a in dto.Answers)
                    {
                        if (string.IsNullOrWhiteSpace(a.AnswerText))
                            return BadRequest("AnswerText cannot be empty");

                        var answer = new Answer
                        {
                            Text = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            QuestionId = question.Id
                        };

                        _context.Answers.Add(answer);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // 🔥 expose real EF Core error instead of silent 500
                return StatusCode(500, ex.ToString());
            }
        }

        // ============================
        // DELETE quiz
        // ============================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
