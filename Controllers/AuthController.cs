
using EduSyncApi.Data;
using EduSyncApi.Models;
using EduSyncApi.Services;
using EduSyncApi.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Edu_Sync_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly PasswordHasher<UserModel> _passwordHasher;

        public AuthController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
            _passwordHasher = new PasswordHasher<UserModel>();
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (await _context.UserModel.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });

            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.UserModel.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email and password are required." });
            var user = await _context.UserModel.SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _authService.GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }
    }
}
