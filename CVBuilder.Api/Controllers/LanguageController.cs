namespace CVBuilder.Api.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageService _service;

        public LanguageController(ILanguageService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LanguageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LanguageDto>>> GetAll([FromQuery] string? search, CancellationToken ct)
        {
            var items = await _service.GetAllAsync(search, ct);
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LanguageDto>> GetById(int id, CancellationToken ct)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item is null) return NotFound();
            return Ok(item);
        }
    }
}
