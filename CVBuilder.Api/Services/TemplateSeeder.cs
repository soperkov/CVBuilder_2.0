namespace CVBuilder.Api.Services
{
    public class TemplateSeeder
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TemplateSeeder> _log;

        public TemplateSeeder(AppDbContext context, IWebHostEnvironment env, ILogger<TemplateSeeder> log)
        {
            _context = context;
            _env = env;
            _log = log;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            var classicPath = Path.Combine(_env.ContentRootPath, "Templates", "cv-classic.css");
            if (!File.Exists(classicPath))
            {
                _log.LogWarning("Template CSS not found at {Path}", classicPath);
                return;
            }

            var css = await File.ReadAllTextAsync(classicPath, ct);

            var existing = await _context.Templates.FirstOrDefaultAsync(t => t.Name == "Classic", ct);

            if (existing is null)
            {
                _context.Templates.Add(new TemplateModel
                {
                    Name = "Classic",
                    Description = "Default two-column resume style",
                    PreviewImageUrl = "",    
                    CssContent = css,
                    IsActive = true
                });
                _log.LogInformation("Inserted 'Classic' template.");
            }
            else
            {
                if (!string.Equals(existing.CssContent, css, StringComparison.Ordinal))
                {
                    existing.CssContent = css;
                    _context.Templates.Update(existing);
                    _log.LogInformation("Updated 'Classic' template CSS.");
                }

                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    _context.Templates.Update(existing);
                }
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
