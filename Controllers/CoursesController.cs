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

        // =====================================================
        // STUDENT / PUBLIC: GET ALL PUBLISHED COURSES
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Where(c => c.IsPublished)
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

        // =====================================================
        // INSTRUCTOR: GET MY COURSES  ✅ (ORIGINAL ENDPOINT)
        // =====================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetMyCourses()
        {
            return await GetInstructorCoursesInternal();
        }

        // =====================================================
        // INSTRUCTOR: GET MY COURSES (ALIAS)
        // =====================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("instructor")]
        public async Task<ActionResult<IEnumerable<CourseReadDto>>> GetInstructorCourses()
        {
            return await GetInstructorCoursesInternal();
        }

        // =====================================================
        // SHARED LOGIC
        // =====================================================
        private async Task<ActionResult<IEnumerable<CourseReadDto>>> GetInstructorCoursesInternal()
        {
            var instructorId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == instructorId)
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

        // =====================================================
        // GET COURSE BY ID (FOR EDIT)
        // =====================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return Ok(course);
        }

        // =====================================================
        // CREATE COURSE
        // =====================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseCreateDto dto)
        {
            var instructorId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

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
                IsPublished = true
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // =====================================================
        // UPDATE COURSE
        // =====================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, CourseCreateDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Title = dto.Title;
            course.ShortDescription = dto.ShortDescription;
            course.LongDescription = dto.LongDescription;
            course.Category = dto.Category;
            course.Difficulty = dto.Difficulty;
            course.Thumbnail = dto.Thumbnail;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================================================
        // DELETE COURSE
        // =====================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
