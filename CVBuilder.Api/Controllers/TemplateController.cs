namespace CVBuilder.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templates;
        public TemplateController(ITemplateService templates) => _templates = templates;

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TemplateDto>> GetById(int id, CancellationToken ct)
        {
            var dto = await _templates.GetByIdAsync(id, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("{id:int}/css")]
        public async Task<IActionResult> GetCss(int id, CancellationToken ct)
        {
            var css = await _templates.GetCssAsync(id, ct);
            if (css is null) return NotFound();

            Response.Headers["Cache-Control"] = "public, max-age=3600";

            return Content(css, "text/css", Encoding.UTF8);
        }
    }
}
