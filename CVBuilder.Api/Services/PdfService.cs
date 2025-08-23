using System.Text;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CVBuilder.Api.Services
{
    public class PdfService : IPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PdfService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<byte[]> GenerateCvPdfAsync(int cvId, int userId, CancellationToken ct = default)
        {
            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .Include(c => c.Template) // <- bitno zbog CSS-a iz baze
                .FirstOrDefaultAsync(c => c.Id == cvId && c.CreatedByUser == userId, ct);

            if (cv == null)
                throw new KeyNotFoundException("CV not found.");

            // 1) Odredi CSS (DB -> fajl -> fallback)
            var css = GetFallbackCss();
            if (cv.Template?.IsActive == true && !string.IsNullOrWhiteSpace(cv.Template.CssContent))
            {
                css = cv.Template.CssContent!;
            }
            else
            {
                var cssPath = Path.Combine(_env.ContentRootPath, "Templates", "cv-classic.css");
                if (File.Exists(cssPath))
                {
                    css = await File.ReadAllTextAsync(cssPath, ct);
                }
            }

            // 2) Generiraj HTML u memoriji
            var html = BuildHtml(cv, css);

            // 3) Puppeteer rendering
            var fetcher = Puppeteer.CreateBrowserFetcher(new BrowserFetcherOptions
            {
                Path = Path.Combine(_env.ContentRootPath, "BrowserCache")
            });
            var revInfo = await fetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = revInfo.GetExecutablePath(),
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            await using var page = await browser.NewPageAsync();
            await page.EmulateMediaTypeAsync(MediaType.Print);
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "10mm",
                    Left = "10mm",
                    Right = "10mm",
                    Bottom = "10mm"
                }
            };

            var bytes = await page.PdfDataAsync(pdfOptions);
            return bytes;
        }

        private static string BuildHtml(CVModel cv, string css)
        {
            string HE(string s) => System.Net.WebUtility.HtmlEncode(s);

            var skillsHtml = string.Join("", (cv.Skills ?? new()).Select(s =>
                $"<li>{HE(s.Name)}</li>"
            ));

            var educationLis = string.Join("", (cv.Education ?? new()).Select(e =>
                $"<li><strong>{HE(e.InstitutionName)}</strong><br/>{FormatDate(e.From)} — {FormatDate(e.To)}<br/>{HE(e.Description ?? "")}</li>"
            ));

            var employmentLis = string.Join("", (cv.Employment ?? new()).Select(e =>
                $"<li><strong>{HE(e.CompanyName)}</strong><br/>{FormatDate(e.From)} — {FormatDate(e.To)}<br/>{HE(e.Description ?? "")}</li>"
            ));

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset='utf-8'/>");
            sb.AppendLine("  <meta name='viewport' content='width=device-width, initial-scale=1'/>");
            sb.AppendLine("  <style>");
            sb.AppendLine(css);
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("  <div class='cv-container'>");

            sb.AppendLine("    <div class='sidebar'>");
            if (!string.IsNullOrWhiteSpace(cv.PhotoUrl))
            {
                sb.AppendLine($"      <img class='profile-photo' src='{HE(cv.PhotoUrl)}' alt='Profile Photo'/>");
            }
            sb.AppendLine("      <h4>Contact</h4>");
            sb.AppendLine($"      <p><strong>Phone:</strong><br/>{HE(cv.PhoneNumber ?? "")}</p>");
            sb.AppendLine($"      <p><strong>Email:</strong><br/>{HE(cv.Email ?? "")}</p>");
            sb.AppendLine("      <h4>Skills</h4>");
            sb.AppendLine($"      <ul class='skills'>{skillsHtml}</ul>");
            sb.AppendLine("    </div>");

            sb.AppendLine("    <div class='main-content'>");
            sb.AppendLine("      <div class='header'>");
            sb.AppendLine($"        <h2 class='full-name'>{HE(cv.FullName ?? "")}</h2>");
            sb.AppendLine("      </div>");
            sb.AppendLine("      <h4>Profile</h4>");
            sb.AppendLine($"      <p>{HE(cv.AboutMe ?? "")}</p>");

            sb.AppendLine("      <h4>Work Experience</h4>");
            sb.AppendLine($"      <ul class='employment'>{employmentLis}</ul>");

            sb.AppendLine("      <h4>Education</h4>");
            sb.AppendLine($"      <ul class='education'>{educationLis}</ul>");
            sb.AppendLine("    </div>");

            sb.AppendLine("  </div>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static string GetFallbackCss() =>
            @"body{font-family:Inter,Arial,sans-serif;color:#333}
              .cv-container{display:flex;gap:24px;max-width:900px;margin:24px auto;border:1px solid #eee;border-radius:8px;padding:24px}
              .sidebar{width:280px}
              .main-content{flex:1}
              h2.full-name{margin:0 0 8px 0}
              h4{margin:24px 0 8px 0;border-bottom:1px solid #eee;padding-bottom:4px}
              ul{list-style:none;padding:0;margin:0}
              li{margin-bottom:8px;line-height:1.4}";

        private static string FormatDate(DateTime? dt)
            => dt.HasValue ? dt.Value.ToString("yyyy-MM") : "";
    }
}
