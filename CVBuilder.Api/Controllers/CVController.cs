namespace CVBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly ICVService _cvService;

        public CVController(ICVService cvService)
        {
            _cvService = cvService;
        }

        [HttpPost]
        [Authorize]
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
        


    }
}
