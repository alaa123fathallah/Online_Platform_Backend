using Back.Data;
using Back.DTOs;
using Back.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProgressController(AppDbContext context)
        {
            _context = context;
        }


        [Authorize(Roles = "Student")]
        [HttpPost("lesson-complete")]
        public async Task<IActionResult> CompleteLesson([FromBody] LessonCompleteDto dto)
        {
            // Check lesson exists
            var lesson = await _context.Lessons.FindAsync(dto.LessonId);
            if (lesson == null)
                return NotFound("Lesson not found");

            // Prevent duplicate completion
            var alreadyCompleted = await _context.LessonsCompletion
                .AnyAsync(lc =>
                    lc.UserId == dto.UserId &&
                    lc.LessonId == dto.LessonId);

            if (alreadyCompleted)
                return BadRequest("Lesson already completed");

            var completion = new LessonCompletion
            {
                UserId = dto.UserId,
                LessonId = dto.LessonId,
                CompletedDate = DateTime.UtcNow
            };

            _context.LessonsCompletion.Add(completion);
            await _context.SaveChangesAsync();

            return Ok("Lesson marked as completed");
        }


        [Authorize(Roles = "Student")]
        [HttpGet("course/{courseId}/user/{userId}")]
        public async Task<ActionResult<CourseProgressDto>> GetCourseProgress(int courseId, int userId)
        {
            // Total lessons in course
            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == courseId);

            // Completed lessons by user
            var completedLessons = await _context.LessonsCompletion
                .CountAsync(lc =>
                    lc.UserId == userId &&
                    _context.Lessons.Any(l =>
                        l.Id == lc.LessonId &&
                        l.CourseId == courseId));

            int progressPercentage = totalLessons == 0
                ? 0
                : (int)Math.Round(
                    (double)completedLessons / totalLessons * 100);

            // Check quizzes passed
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            bool allQuizzesPassed = true;

            foreach (var quiz in quizzes)
            {
                var passed = await _context.QuizAttempts
                    .AnyAsync(a =>
                        a.UserId == userId &&
                        a.QuizId == quiz.Id &&
                        a.Score >= quiz.PassingScore);

                if (!passed)
                {
                    allQuizzesPassed = false;
                    break;
                }
            }

            return Ok(new CourseProgressDto
            {
                CourseId = courseId,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercentage = progressPercentage,
                AllQuizzesPassed = allQuizzesPassed
            });
        }
    }
}