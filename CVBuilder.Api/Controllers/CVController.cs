namespace CVBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CVController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCv(CreateCvDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var cv = new CVModel
            {
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                AboutMe = dto.AboutMe,
                PhotoUrl = dto.PhotoUrl,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUser = userId,
                TemplateId = dto.TemplateId
            };

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            return Ok(new { cv.Id });
        }
    }
}
