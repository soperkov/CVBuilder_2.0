namespace CVBuilder.Api.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly AppDbContext _context;
        public TemplateService(AppDbContext context) => _context = context;

        public async Task<TemplateDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    PreviewImageUrl = t.PreviewImageUrl,
                    IsActive = t.IsActive,
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<string?> GetCssAsync(int id, CancellationToken ct = default)
        {
            var css = await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == id && t.IsActive)
                .Select(t => t.CssContent)
                .FirstOrDefaultAsync(ct);

            return string.IsNullOrWhiteSpace(css) ? null : css;
        }
    }
}
