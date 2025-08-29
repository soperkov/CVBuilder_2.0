namespace CVBuilder.Api.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templates;

        public TemplateController(ITemplateService templates)
        {
            _templates = templates;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateOptionDto>>> GetAll(CancellationToken ct)
        {
            var items = await _templates.GetAllAsync(ct);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            // Thumbs are served from: wwwroot/templates/{Name}.png
            var result = items.Select(t => new TemplateOptionDto
            {
                Id = t.Id,
                Name = t.Name,
                ThumbUrl = $"{baseUrl}/templates/{t.Name}.png"
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TemplateOptionDto>> GetById(int id, CancellationToken ct)
        {
            var t = await _templates.GetByIdAsync(id, ct);
            if (t is null) return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var dto = new TemplateOptionDto
            {
                Id = t.Id,
                Name = t.Name,
                ThumbUrl = $"{baseUrl}/templates/{t.Name}.png"
            };

            return Ok(dto);
        }
    }

}

