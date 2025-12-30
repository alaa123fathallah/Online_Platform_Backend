namespace Back.DTOs
{
    public class UserCreateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }   // raw password (will hash later)
        public string Role { get; set; }
    }
}