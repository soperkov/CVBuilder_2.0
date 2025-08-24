using Microsoft.Playwright;

namespace CVBuilder.Api.Services
{
    public class PlaywrightPdfService : IPlaywrightPdfService
    {
        private readonly AppDbContext _context;
        private readonly ITemplateRenderService _renderer;

        public PlaywrightPdfService(AppDbContext context, ITemplateRenderService renderer)
        {
            _context = context;
            _renderer = renderer;
        }

        public async Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(int cvId, int userId, CancellationToken ct = default)
        {
            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .FirstOrDefaultAsync(c => c.Id == cvId && c.CreatedByUser == userId, ct);

            if (cv == null) throw new KeyNotFoundException("CV not found or not yours.");

            string? templateName = null;
            if (cv.TemplateId.HasValue)
            {
                templateName = await _context.Templates
                    .Where(t => t.Id == cv.TemplateId.Value)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync(ct);
            }

            var html = await _renderer.RenderAsync(templateName, cv, ct);

            using var pw = await Playwright.CreateAsync();
            await using var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });

            var bytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "10mm", Right = "10mm", Bottom = "10mm", Left = "10mm" }
            });

            var safeName = !string.IsNullOrWhiteSpace(cv.CVName) ? cv.CVName.Trim() : $"CV_{cv.Id}";
            foreach (var ch in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(ch, '_');

            return (bytes, safeName + ".pdf");
        }
    }
}
