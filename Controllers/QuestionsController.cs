using Back.Data;
using Back.DTOs;
using Back.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // ✅ GET QUESTION BY ID (ADDED)
        // ===============================
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            return Ok(new QuestionReadDto
            {
                Id = question.Id,
                QuestionText = question.Text,
                QuestionType = question.Type.ToString(),
                Answers = question.Answers.Select(a => new AnswerReadDto
                {
                    Id = a.Id,
                    AnswerText = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            });
        }

        // ===============================
        // UPDATE QUESTION
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, QuestionCreateDto dto)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            question.Text = dto.QuestionText;
            question.Type = Enum.Parse<QuestionType>(dto.QuestionType);

            // Remove old answers
            _context.Answers.RemoveRange(question.Answers);

            // Add new answers
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
            return Ok();
        }

        // ===============================
        // DELETE QUESTION
        // ===============================
        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            _context.Answers.RemoveRange(question.Answers);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
