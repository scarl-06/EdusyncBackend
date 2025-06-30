using EduSyncApi.Data;
using EduSyncApi.Models;
using EduSyncApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EventHubService _eventHubService;
        public ResultsController(AppDbContext context, EventHubService eventHubService)
        {
            _context = context;
            _eventHubService = eventHubService;
        }

        // GET: api/Results
        [HttpGet]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<IEnumerable<ResultReadDTO>>> GetResults()
        {
            return await _context.ResultModel
                .Select(r => new ResultReadDTO
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    UserId = r.UserId,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate
                }).ToListAsync();
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<ResultReadDTO>> GetResult(Guid id)
        {
            var result = await _context.ResultModel.FindAsync(id);
            if (result == null) return NotFound();

            return new ResultReadDTO
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                UserId = result.UserId,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };
        }

        // POST: api/Results
        [HttpPost]
        //[Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<ResultReadDTO>> PostResult(ResultCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.AssessmentId != null &&
                !await _context.Assessments.AnyAsync(a => a.AssessmentId == dto.AssessmentId))
                return Conflict("Assessment with given ID does not exist.");

            if (dto.UserId != null &&
                !await _context.UserModel.AnyAsync(u => u.UserId == dto.UserId))
                return Conflict("User with given ID does not exist.");

            var result = new ResultModel
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                Score = dto.Score,
                AttemptDate = dto.AttemptDate
            };

            _context.ResultModel.Add(result);
            
            await _context.SaveChangesAsync();
            await _eventHubService.SendAsync(new
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                UserId = result.UserId,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            });


            var resultDto = new ResultReadDTO
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                UserId = result.UserId,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };

            return CreatedAtAction(nameof(GetResult), new { id = result.ResultId }, resultDto);
        }

        // PUT: api/Results/5
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> PutResult(Guid id, ResultCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _context.ResultModel.FindAsync(id);
            if (result == null) return NotFound();

            if (dto.AssessmentId != null &&
                !await _context.Assessments.AnyAsync(a => a.AssessmentId == dto.AssessmentId))
                return Conflict("Assessment with given ID does not exist.");

            if (dto.UserId != null &&
                !await _context.UserModel.AnyAsync(u => u.UserId == dto.UserId))
                return Conflict("User with given ID does not exist.");

            result.AssessmentId = dto.AssessmentId;
            result.UserId = dto.UserId;
            result.Score = dto.Score;
            result.AttemptDate = dto.AttemptDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ResultModel.Any(r => r.ResultId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Results/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await _context.ResultModel.FindAsync(id);
            if (result == null) return NotFound();

            _context.ResultModel.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
