using EduSyncApi.Data;
using EduSyncApi.Models;
using EduSyncApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSyncWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<IEnumerable<CourseReadDTO>>> GetCourses()
        {
            var courses = await _context.Courses
                .Select(c => new CourseReadDTO
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    MediaUrl = c.MediaUrl
                }).ToListAsync();

            return Ok(courses);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<CourseDetailDTO>> GetCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Assessment)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var result = new CourseDetailDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl,
                Assessments = course.Assessment?
                    .Select(a => new AssessmentSummaryDTO
                    {
                        AssessmentId = a.AssessmentId,
                        Title = a.Title,
                        MaxScore = a.MaxScore
                    }).ToList(),
                Instructor = course.Instructor == null ? null : new UserDto
                {
                    UserId = course.Instructor.UserId,
                    FullName = course.Instructor.Name,
                    Email = course.Instructor.Email
                }
            };

            return Ok(result);
        }

        // POST: api/Course
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]

        public async Task<ActionResult<CourseReadDTO>> CreateCourse(CourseCreateDTO courseDto)
        {
            if (courseDto.InstructorId.HasValue &&
                !await _context.UserModel.AnyAsync(u => u.UserId == courseDto.InstructorId.Value))
            {
                return Conflict("Instructor with given ID does not exist.");
            }

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = courseDto.Title,
                Description = courseDto.Description,
                InstructorId = courseDto.InstructorId,
                MediaUrl = courseDto.MediaUrl
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var courseReadDto = new CourseReadDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, courseReadDto);
        }

        // PUT: api/Course/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> UpdateCourse(Guid id, CourseCreateDTO dto)
        {
            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            if (dto.InstructorId.HasValue &&
                !await _context.UserModel.AnyAsync(u => u.UserId == dto.InstructorId.Value))
            {
                return Conflict("Instructor with given ID does not exist.");
            }

            existingCourse.Title = dto.Title;
            existingCourse.Description = dto.Description;
            existingCourse.InstructorId = dto.InstructorId;
            existingCourse.MediaUrl = dto.MediaUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Check foreign key constraint (Assessments)
            bool hasAssessments = await _context.Assessments.AnyAsync(a => a.CourseId == id);
            if (hasAssessments)
            {
                return Conflict("Cannot delete course with associated assessments.");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
