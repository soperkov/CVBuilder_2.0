namespace CVBuilder.Api.Services
{
    public class TemplateRenderService : ITemplateRenderService
    {
        private readonly IServiceProvider _sp;
        private readonly ILoggerFactory _lf;
        private readonly ITemplateCatalog _catalog;
        private readonly IPhotoDataUriService _photoDataUri;

        public TemplateRenderService(IServiceProvider sp, ILoggerFactory lf, ITemplateCatalog catalog, IPhotoDataUriService photoDataUri)
        {
            _sp = sp;
            _lf = lf;
            _catalog = catalog;
            _photoDataUri = photoDataUri;
        }

        public async Task<string> RenderAsync(string? templateName, CVModel model, CancellationToken ct = default)
        {
            var componentType =
                (templateName != null && _catalog.TryGet(templateName, out var t)) ? t
                : _catalog.All.Values.FirstOrDefault()
                  ?? throw new InvalidOperationException("No template components discovered.");

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
    }
}
