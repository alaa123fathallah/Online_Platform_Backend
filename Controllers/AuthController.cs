using Back.Data;
using Back.DTOs;
using Back.Entities;
using Back.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ===============================
        // POST: api/auth/register
        // ===============================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            // ✅ Normalize + validate role
            var requestedRole = dto.Role?.Trim();

            string role;
            if (string.Equals(requestedRole, "Instructor", StringComparison.OrdinalIgnoreCase))
            {
                role = "Instructor";
            }
            else
            {
                role = "Student"; // safe default
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                HashedPassword = PasswordHasher.Hash(dto.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully",
                role = role
            });
        }

        // ===============================
        // POST: api/auth/login
        // ===============================
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!PasswordHasher.Verify(dto.Password, user.HashedPassword))
                return Unauthorized("Invalid credentials");

            // ===============================
            // JWT CLAIMS (ROLE INCLUDED)
            // ===============================
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var expiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:DurationInMinutes"])
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return Ok(new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt
            });
        }
    }
}
