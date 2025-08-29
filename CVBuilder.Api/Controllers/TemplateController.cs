namespace CVBuilder.Api.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templates;

        public TemplateController(ITemplateService templates)
        {
            _templates = templates;
        }

        [HttpGet]
        public async Task<ActionResult<List<TemplateDto>>> GetAll(CancellationToken ct)
        {
            var list = await _templates.GetAllAsync(ct);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TemplateDto>> GetById(int id, CancellationToken ct)
        {
            var dto = await _templates.GetAsync(id, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [AllowAnonymous] // optional: allow front-end preview without auth
        [HttpGet("{id:int}/preview-html")]
        public async Task<IActionResult> PreviewHtml(int id, CancellationToken ct)
        {
            try
            {
                var html = await _templates.RenderPreviewHtmlAsync(id, ct);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
