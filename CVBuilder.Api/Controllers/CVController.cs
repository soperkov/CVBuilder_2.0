using CVBuilder.Api.Services;
using CVBuilder.Core.Interfaces;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace CVBuilder.Api.Controllers
{
    [Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly ICVService _cvService;
        private readonly IPlaywrightPdfService _pdf;

        public CVController(ICVService cvService, IPlaywrightPdfService pdf)
        {
            _cvService = cvService;
            _pdf = pdf;
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
        public async Task<IActionResult> DownloadPdf(int id, CancellationToken ct)
        {
            int userId = GetUserId();
            var (bytes, name) = await _pdf.GenerateByCvIdAsync(id, userId, ct);
            return File(bytes, "application/pdf", name);
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> Preview(int id, [FromServices] PdfGenerator renderer)
        {
            var userId = GetUserId();                 
            var cv = await _cvService.GetCvForRenderAsync(id, userId);
            if (cv is null) return NotFound();

            var html = await renderer.RenderCVToHtmlAsync(cv); 
            return Content(html, "text/html; charset=utf-8");
        }
    }
}

