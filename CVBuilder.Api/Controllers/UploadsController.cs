namespace CVBuilder.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IUploadsService _uploads;
        private readonly IWebHostEnvironment _env;

        public UploadsController(IUploadsService uploads, IWebHostEnvironment env)
        {
            _uploads = uploads;
            _env = env;
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

        [AllowAnonymous]
        [HttpGet("photo")]
        public async Task<IActionResult> GetPhoto([FromQuery] string path, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("path is required");
            var file = await _uploads.TryOpenPhotoAsync(path, ct);
            if (file == null) return NotFound();
            return File(file.Stream, file.ContentType);
        }

        [AllowAnonymous]
        [HttpGet("preview/{id:int}")]
        public IActionResult GetCvPreview(int id)
        {
            var path = Path.Combine(_env.WebRootPath, "cv_previews", $"{id}.png");
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return File(fileStream, "image/png");
        }
    }
}