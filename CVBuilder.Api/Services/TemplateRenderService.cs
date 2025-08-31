namespace CVBuilder.Api.Services
{
    public class TemplateRenderService : ITemplateRenderService
    {
        private readonly IServiceProvider _sp;
        private readonly ILoggerFactory _lf;
        private readonly ITemplateCatalog _catalog;
        private readonly IPhotoDataUriService _photoDataUri;
        private readonly ILogger<TemplateRenderService> _log;

        public TemplateRenderService(
            IServiceProvider sp,
            ILoggerFactory lf,
            ITemplateCatalog catalog,
            IPhotoDataUriService photoDataUri)
        {
            _sp = sp;
            _lf = lf;
            _catalog = catalog;
            _photoDataUri = photoDataUri;
            _log = lf.CreateLogger<TemplateRenderService>();
        }

        public async Task<string> RenderAsync(string? templateName, CVModel model, CancellationToken ct = default)
        {
            // 1) Prefer the CV's own template name
            var effectiveName = model?.Template?.Name;

            // 2) Otherwise use explicitly-passed templateName
            if (string.IsNullOrWhiteSpace(effectiveName))
                effectiveName = templateName;

            // 3) Resolve component (robust name matching before falling back)
            var componentType = ResolveComponentType(effectiveName);

            // shallow copy so EF-tracked entity isn’t mutated
            var vm = new CVModel
            {
                CVName = model.CVName,
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                AboutMe = model.AboutMe,
                Address = model.Address,
                WebPage = model.WebPage,
                JobTitle = model.JobTitle,
                TemplateId = model.TemplateId,
                PhotoUrl = model.PhotoUrl,
                Skills = model.Skills,
                Education = model.Education,
                Employment = model.Employment,
                Language = model.Language,
                Template = model.Template
            };

            // inline photo as data: URI if available
            var dataUri = await _photoDataUri.ToDataUriAsync(model.PhotoUrl, ct);
            if (!string.IsNullOrWhiteSpace(dataUri))
                vm.PhotoUrl = dataUri;

            await using var renderer = new HtmlRenderer(_sp, _lf);

            return await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var pv = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = vm });
                var result = await renderer.RenderComponentAsync(componentType, pv);
                return result.ToHtmlString();
            });
        }

        // --- helpers --------------------------------------------------------

        private Type ResolveComponentType(string? name)
        {
            // direct hit first (catalog is OrdinalIgnoreCase)
            if (!string.IsNullOrWhiteSpace(name) && _catalog.TryGet(name, out var direct))
                return direct;

            if (!string.IsNullOrWhiteSpace(name))
            {
                // tolerant match: ignore spaces, dashes, underscores, case
                var norm = Normalize(name);
                var matchKey = _catalog.All.Keys.FirstOrDefault(k => Normalize(k) == norm);
                if (matchKey != null && _catalog.TryGet(matchKey, out var matched))
                    return matched;

                _log.LogWarning("Template name '{Name}' not found. Falling back to first available template.", name);
            }

            // final fallback to first discovered
            var first = _catalog.All.Values.FirstOrDefault();
            if (first == null)
                throw new InvalidOperationException("No template components discovered.");

            return first;
        }

        private static string Normalize(string s)
        {
            // strip spaces, dashes, underscores; lower case
            Span<char> buffer = stackalloc char[s.Length];
            var idx = 0;
            foreach (var ch in s)
            {
                if (ch == ' ' || ch == '-' || ch == '_') continue;
                buffer[idx++] = char.ToLowerInvariant(ch);
            }
            return new string(buffer[..idx]);
        }
    }
}
