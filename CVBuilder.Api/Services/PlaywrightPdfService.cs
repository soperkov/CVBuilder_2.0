namespace CVBuilder.Api.Services
{
    public class PlaywrightPdfService : IPlaywrightPdfService
    {
        private readonly ICVService _cvService;
        private readonly PdfGenerator _pdfGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        public PlaywrightPdfService(
            ICVService cvService,
            PdfGenerator pdfGenerator,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env)
        {
            _cvService = cvService;
            _pdfGenerator = pdfGenerator;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        // Uses current request to determine baseUrl
        public async Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(
            int cvId, int userId, CancellationToken ct = default)
        {
            var cv = await _cvService.GetCvForRenderAsync(cvId, userId);
            if (cv == null) throw new KeyNotFoundException("CV not found or not yours.");

            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HttpContext available.");
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            var html = await _pdfGenerator.RenderCVToHtmlAsync(cv, baseUrl, ct);

            using var pw = await Playwright.CreateAsync();
            await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.EmulateMediaAsync(new() { Media = Media.Screen });

            // The key fix: Take a full-page screenshot
            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = true, // Capture the entire A4 page
                Type = ScreenshotType.Png
            });

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "10mm", Right = "10mm", Bottom = "10mm", Left = "10mm" }
            });

            var safeName = string.IsNullOrWhiteSpace(cv.CVName) ? $"CV_{cv.Id}" : cv.CVName.Trim();
            foreach (var ch in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(ch, '_');

            var previewsDir = Path.Combine(_env.WebRootPath, "cv_previews");
            Directory.CreateDirectory(previewsDir);
            await File.WriteAllBytesAsync(Path.Combine(previewsDir, $"{cv.Id}.png"), screenshotBytes, ct);

            return (pdfBytes, safeName + ".pdf");
        }

        // Explicit baseUrl overload (used by your controller after create/update)
        public async Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(
            int cvId, int userId, string baseUrl, CancellationToken ct = default)
        {
            var cv = await _cvService.GetCvForRenderAsync(cvId, userId);
            if (cv == null) throw new KeyNotFoundException("CV not found or not yours.");

            var html = await _pdfGenerator.RenderCVToHtmlAsync(cv, baseUrl, ct);

            using var pw = await Playwright.CreateAsync();
            await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.EmulateMediaAsync(new() { Media = Media.Print });

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "10mm", Right = "10mm", Bottom = "10mm", Left = "10mm" }
            });

            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = true, // Capture the entire A4 page
                Type = ScreenshotType.Png,
                Clip = new Clip { X = 0, Y = 0, Width = 800, Height = 1131 }, 
            });

            var safeName = string.IsNullOrWhiteSpace(cv.CVName) ? $"CV_{cv.Id}" : cv.CVName.Trim();
            foreach (var ch in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(ch, '_');

            var previewsDir = Path.Combine(_env.WebRootPath, "cv_previews");
            Directory.CreateDirectory(previewsDir);
            await File.WriteAllBytesAsync(Path.Combine(previewsDir, $"{cv.Id}.png"), screenshotBytes, ct);

            return (pdfBytes, safeName + ".pdf");
        }

        public async Task<(byte[] Bytes, string FileName)> GeneratePdfOnlyByCvIdAsync(
    int cvId, int userId, CancellationToken ct = default)
        {
            var cv = await _cvService.GetCvForRenderAsync(cvId, userId);
            if (cv == null) throw new KeyNotFoundException("CV not found or not yours.");

            // Build a base URL from current request
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HttpContext available.");
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            // Render HTML for the correct template
            var html = await _pdfGenerator.RenderCVToHtmlAsync(cv, baseUrl, ct);

            using var pw = await Playwright.CreateAsync();
            await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.EmulateMediaAsync(new() { Media = Media.Print });

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "10mm", Right = "10mm", Bottom = "10mm", Left = "10mm" }
            });

            var safeName = string.IsNullOrWhiteSpace(cv.CVName) ? $"CV_{cv.Id}" : cv.CVName.Trim();
            foreach (var ch in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(ch, '_');

            return (pdfBytes, safeName + ".pdf");
        }

    }
}