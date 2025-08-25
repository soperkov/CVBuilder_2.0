using Microsoft.Playwright;

namespace CVBuilder.Api.Services
{
    public class PlaywrightPdfService : IPlaywrightPdfService
    {
        private readonly ICVService _cvService;
        private readonly ITemplateRenderService _renderer;

        public PlaywrightPdfService(ICVService cvService, ITemplateRenderService renderer)
        {
            _cvService = cvService;
            _renderer = renderer;
        }

        public async Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(int cvId, int userId, CancellationToken ct = default)
        {
            var cv = await _cvService.GetCvForRenderAsync(cvId, userId);
            if (cv == null) throw new KeyNotFoundException("CV not found or not yours.");

            var templateName = cv.Template?.Name;

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

            var safeName = string.IsNullOrWhiteSpace(cv.CVName) ? $"CV_{cv.Id}" : cv.CVName.Trim();
            foreach (var ch in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(ch, '_');

            return (bytes, safeName + ".pdf");
        }
    }
}
