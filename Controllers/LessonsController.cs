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
    public class LessonsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LessonsController(AppDbContext context)
        {
            _context = context;
        }



        // GET: api/lessons/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<LessonReadDto>>> GetLessonsByCourse(int courseId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .Select(l => new LessonReadDto
                {
                    Id = l.Id,
                    CourseId = l.CourseId,
                    Title = l.Title,
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    Order = l.Order,
                    EstimatedDuration = l.EstimatedDuration,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(lessons);
        }

        // GET: api/lessons/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LessonReadDto>> GetLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null)
                return NotFound();

            return Ok(new LessonReadDto
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                Title = lesson.Title,
                Content = lesson.Content,
                VideoUrl = lesson.VideoUrl,
                Order = lesson.Order,
                EstimatedDuration = lesson.EstimatedDuration,
                CreatedAt = lesson.CreatedAt
            });
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<LessonReadDto>> CreateLesson([FromBody] LessonCreateDto dto)
        {
            // Safety check
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                return BadRequest("Course does not exist");

            var lesson = new Lesson
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Content = dto.Content,
                VideoUrl = dto.VideoUrl,
                Order = dto.Order,
                EstimatedDuration = dto.EstimatedDuration,
                CreatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLesson),
                new { id = lesson.Id },
                new LessonReadDto
                {
                    Id = lesson.Id,
                    CourseId = lesson.CourseId,
                    Title = lesson.Title,
                    Content = lesson.Content,
                    VideoUrl = lesson.VideoUrl,
                    Order = lesson.Order,
                    EstimatedDuration = lesson.EstimatedDuration,
                    CreatedAt = lesson.CreatedAt
                }
            );
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonUpdateDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null)
                return NotFound();

            lesson.Title = dto.Title;
            lesson.Content = dto.Content;
            lesson.VideoUrl = dto.VideoUrl;
            lesson.Order = dto.Order;
            lesson.EstimatedDuration = dto.EstimatedDuration;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null)
                return NotFound();

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}