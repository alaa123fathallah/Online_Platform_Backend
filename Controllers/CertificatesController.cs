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
    public class CertificatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CertificatesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateCertificate([FromBody] CertificateGenerateDto dto)
        {
            // 1. Check enrollment
            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == dto.UserId && e.CourseId == dto.CourseId);

            if (!enrolled)
                return BadRequest("User is not enrolled in this course");

            // 2. Check lesson completion
            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == dto.CourseId);

            var completedLessons = await _context.LessonsCompletion
                .CountAsync(lc =>
                    lc.UserId == dto.UserId &&
                    _context.Lessons.Any(l =>
                        l.Id == lc.LessonId && l.CourseId == dto.CourseId));

            if (completedLessons < totalLessons)
                return BadRequest("Not all lessons completed");

            // 3. Check quizzes passed
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == dto.CourseId)
                .ToListAsync();

            foreach (var quiz in quizzes)
            {
                var passed = await _context.QuizAttempts
                    .AnyAsync(a =>
                        a.QuizId == quiz.Id &&
                        a.UserId == dto.UserId &&
                        a.Score >= quiz.PassingScore);

                if (!passed)
                    return BadRequest("Not all quizzes passed");
            }

            // 4. Prevent duplicate certificate
            var alreadyGenerated = await _context.Certificates
                .AnyAsync(c =>
                    c.UserId == dto.UserId &&
                    c.CourseId == dto.CourseId);

            if (alreadyGenerated)
                return BadRequest("Certificate already generated");

            // 5. Generate certificate
            var certificate = new Certificate
            {
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                DownloadUrl = $"certificates/{dto.UserId}_{dto.CourseId}.pdf",
                GeneratedAt = DateTime.UtcNow
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return Ok("Certificate generated successfully");
        }


        [Authorize(Roles = "Student")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CertificateReadDto>>> GetUserCertificates(int userId)
        {
            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .Include(c => c.Course)
                .Select(c => new CertificateReadDto
                {
                    Id = c.Id,
                    CourseId = c.CourseId,
                    CourseTitle = c.Course.Title,
                    DownloadUrl = c.DownloadUrl,
                    GeneratedAt = c.GeneratedAt
                })
                .ToListAsync();

            return Ok(certificates);
        }
    }
}