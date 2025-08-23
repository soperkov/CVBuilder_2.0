using CVBuilder.Api.Services;
using CVBuilder.Core.Interfaces;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace CVBuilder.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly ICVService _cvService;
        private readonly IPdfService _pdfService;

        public CVController(ICVService cvService, IPdfService pdfService)
        {
            _cvService = cvService;
            _pdfService = pdfService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token.");
            return int.Parse(userIdClaim.Value);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCV([FromBody] CreateCVDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            try
            {
                var cvId = await _cvService.CreateCvAsync(dto, userId);
                return Ok(new { Id = cvId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<CVSummaryDto>>> GetMyCvs()
        {
            int userId = GetUserId();
            var result = await _cvService.GetMyCvsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CVSummaryDto>> GetById(int id)
        {
            try
            {
                int userId = GetUserId();
                var cv = await _cvService.GetCvByIdAsync(id, userId);
                if (cv == null)
                    return NotFound("CV not found or you do not have access to it.");

                return Ok(cv);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCv(int id, [FromBody] CreateCVDto dto)
        {
            try
            {
                int userId = GetUserId();
                var updated = await _cvService.UpdateCvAsync(id, dto, userId);
                if (!updated)
                    return NotFound("CV not found or not yours.");
                return NoContent(); // 204
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCv(int id)
        {
            try
            {
                int userId = GetUserId();
                var deleted = await _cvService.DeleteCvAsync(id, userId);
                if (!deleted)
                    return NotFound("CV not found or not yours.");
                return NoContent(); // 204
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMany([FromQuery] string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return BadRequest("ids query is required");

            var parsed = ids
              .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
              .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
              .Where(id => id.HasValue)
              .Select(id => id!.Value)
              .ToArray();

            if (parsed.Length == 0)
                return BadRequest("No valid ids.");

            await _cvService.DeleteManyAsync(parsed);
            return NoContent();
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(int id, CancellationToken ct)
        {
            int userId = GetUserId();

            var bytes = await _pdfService.GenerateCvPdfAsync(id, userId, ct);

            var cv = await _cvService.GetCvByIdAsync(id, userId);
            if (cv == null) return NotFound();

            var safeName = string.IsNullOrWhiteSpace(cv.CVName) ? $"CV_{id}" : cv.CVName;

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safeName = safeName.Replace(c, '_');
            }

            var fileName = $"{safeName}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

    }
}
