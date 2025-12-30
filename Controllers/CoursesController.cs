using Back.Data;
using Back.DTOs;
using Back.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL COURSES (PUBLIC)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    ShortDescription = c.ShortDescription,
                    Category = c.Category,
                    Difficulty = c.Difficulty,
                    IsPublished = c.IsPublished,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(courses);
        }

        // GET COURSE BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseReadDto>> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return Ok(new CourseReadDto
            {
                Id = course.Id,
                Title = course.Title,
                ShortDescription = course.ShortDescription,
                Category = course.Category,
                Difficulty = course.Difficulty,
                IsPublished = course.IsPublished,
                CreatedAt = course.CreatedAt
            });
        }

        // GET COURSES CREATED BY LOGGED-IN INSTRUCTOR
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetMyCourses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == userId)
                .Select(c => new CourseReadDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    ShortDescription = c.ShortDescription,
                    Category = c.Category,
                    Difficulty = c.Difficulty,
                    IsPublished = c.IsPublished,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(courses);
        }

        // CREATE COURSE
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<CourseReadDto>> CreateCourse(CourseCreateDto dto)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var course = new Course
            {
                Title = dto.Title,
                ShortDescription = dto.ShortDescription,
                LongDescription = dto.LongDescription,
                Category = dto.Category,
                Difficulty = dto.Difficulty,
                Thumbnail = dto.Thumbnail,
                CreatedBy = instructorId,
                CreatedAt = DateTime.UtcNow,
                IsPublished = true // ✅ FIX
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, new CourseReadDto
            {
                Id = course.Id,
                Title = course.Title,
                ShortDescription = course.ShortDescription,
                Category = course.Category,
                Difficulty = course.Difficulty,
                IsPublished = course.IsPublished,
                CreatedAt = course.CreatedAt
            });
        }
    }
}
