namespace CVBuilder.Api.Services
{
    public sealed class TemplateService : ITemplateService
    {
        private readonly AppDbContext _context;
        private readonly ITemplateCatalog _catalog;
        private readonly ITemplateRenderService _render;
        private readonly IDummyCvFactory _dummy;

        public TemplateService(
            AppDbContext context,
            ITemplateCatalog catalog,
            ITemplateRenderService render,
            IDummyCvFactory dummy)
        {
            _context = context;
            _catalog = catalog;
            _render = render;
            _dummy = dummy;
        }

        public async Task<List<TemplateDto>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _context.Templates
                .OrderBy(t => t.Name)
                .Select(t => new TemplateDto { Id = t.Id, Name = t.Name })
                .ToListAsync(ct);

            return items;
        }

        public async Task<TemplateDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var item = await _context.Templates
                .Where(t => t.Id == id)
                .Select(t => new TemplateDto { Id = t.Id, Name = t.Name })
                .FirstOrDefaultAsync(ct);
            return item;
        }

        public async Task<string> RenderPreviewHtmlAsync(int id, CancellationToken ct = default)
        {
            var t = await _context.Templates.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (t is null)
                throw new KeyNotFoundException("Template not found.");

            // Make sure the catalog actually has this template component
            if (!_catalog.TryGet(t.Name, out var _))
                throw new InvalidOperationException($"Template '{t.Name}' not registered in catalog.");

            // Generate a dummy CV and render that template
            var model = _dummy.Create();
            model.TemplateId = id;

            // Your TemplateRenderService will in-line the photo through IPhotoDataUriService
            var html = await _render.RenderAsync(t.Name, model, ct);
            return html;
        }
    }
}
