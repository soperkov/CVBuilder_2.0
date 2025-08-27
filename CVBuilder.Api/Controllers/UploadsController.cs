namespace CVBuilder.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IUploadsService _uploads;

        public UploadsController(IUploadsService uploads)
        {
            _uploads = uploads;
        }

        [HttpPost("photo")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file is null) return BadRequest("No file.");
            try
            {
                var result = await _uploads.SavePrivatePhotoAsync(file, ct);
                return Ok(new { path = result.Path, fileName = result.FileName, size = result.Size, contentType = result.ContentType });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("photo")]
        public async Task<IActionResult> GetPhoto([FromQuery] string path, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("path is required");
            var file = await _uploads.TryOpenPhotoAsync(path, ct);
            if (file == null) return NotFound();
            return File(file.Stream, file.ContentType);
        }
    }
}