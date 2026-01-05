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
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // ENROLL IN COURSE (STUDENT)
        // ============================
        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Invalid token");

            int userId = int.Parse(userIdClaim.Value);

            // Check user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return BadRequest("User does not exist");

            // Check course exists
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                return BadRequest("Course does not exist");

            // Prevent duplicate enrollment
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == dto.CourseId);

            if (alreadyEnrolled)
                return BadRequest("User already enrolled in this course");

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = dto.CourseId,
                EnrolledAt = DateTime.UtcNow,
                Status = "Active"
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok("Enrollment successful");
        }

        // =========================================
        // GET MY ENROLLMENTS + REAL PROGRESS
        // =========================================
        [Authorize(Roles = "Student")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<EnrollmentReadDto>>> GetMyEnrollments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Invalid token");

            int userId = int.Parse(userIdClaim.Value);

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .Select(e => new EnrollmentReadDto
                {
                    EnrollmentId = e.Id,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,

                    TotalLessons = e.Course.Lessons.Count(),

                    CompletedLessons = _context.LessonsCompletion
                        .Count(lc =>
                            lc.UserId == userId &&
                            lc.Lesson.CourseId == e.CourseId
                        ),

                    ProgressPercent =
                        e.Course.Lessons.Count() == 0
                            ? 0
                            : (int)(
                                (double)_context.LessonsCompletion
                                    .Count(lc =>
                                        lc.UserId == userId &&
                                        lc.Lesson.CourseId == e.CourseId
                                    )
                                / e.Course.Lessons.Count() * 100
                              )
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // =========================================
        // INSTRUCTOR / ADMIN – COURSE ENROLLMENTS
        // =========================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<EnrollmentReadDto>>> GetCourseEnrollments(int courseId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Course)
                .Select(e => new EnrollmentReadDto
                {
                    EnrollmentId = e.Id,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title
                })
                .ToListAsync();

            return Ok(enrollments);
        }
        // ===============================
        // INSTRUCTOR: GET COURSE STUDENTS (WITH USER INFO)
        // ===============================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("course/{courseId}/students")]
        public async Task<IActionResult> GetCourseStudents(int courseId)
        {
            var students = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.User)
                .Select(e => new
                {
                    UserId = e.User.Id,
                    FullName = e.User.FullName,
                    Email = e.User.Email,
                    EnrolledAt = e.EnrolledAt
                })
                .ToListAsync();

            return Ok(students);
        }

    }
}
