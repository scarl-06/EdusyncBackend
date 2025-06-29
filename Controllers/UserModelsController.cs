using EduSyncApi.Data;
using EduSyncApi.Models;
using EduSyncApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Sync_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserModels
        [HttpGet]
        //[Authorize(Policy ="RequireAdminRole")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUserModels()
        {
            var users = await _context.UserModel
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.Name,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/UserModels/{id}
        [HttpGet("{id}")]
        //[Authorize(Policy = "RequireAdminOrStudentRole")]
        public async Task<ActionResult<UserDetailDto>> GetUserModel(Guid id)
        {
            var user = await _context.UserModel
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var userDto = new UserDetailDto
            {
                UserId = user.UserId,
                FullName = user.Name,
                Email = user.Email,
                Courses = user.Courses.Select(c => new CourseReadDTO
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    MediaUrl = c.MediaUrl
                }).ToList()
            };

            return Ok(userDto);
        }

        // PUT: api/UserModels/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> PutUserModel(Guid id, UserDto userDTO)
        {
            if (id != userDTO.UserId)
                return BadRequest();

            var userModel = await _context.UserModel.FindAsync(id);
            if (userModel == null)
                return NotFound();

            userModel.Name = userDTO.FullName;
            userModel.Email = userDTO.Email;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserModelExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/UserModels
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<UserModel>> PostUserModel(UserDto userDto)
        {
            var userModel = new UserModel
            {
                UserId = userDto.UserId != Guid.Empty ? userDto.UserId : Guid.NewGuid(),
                Name = userDto.FullName,
                Email = userDto.Email,
                Role = "User", // default or assign as needed
                PasswordHash = "" // initialize properly or remove if not needed here
            };

            _context.UserModel.Add(userModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserModel), new { id = userModel.UserId }, userModel);
        }

        // DELETE: api/UserModels/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteUserModel(Guid id)
        {
            var userModel = await _context.UserModel.FindAsync(id);
            if (userModel == null)
                return NotFound();

            _context.UserModel.Remove(userModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserModelExists(Guid id)
        {
            return _context.UserModel.Any(e => e.UserId == id);
        }
    }
}
