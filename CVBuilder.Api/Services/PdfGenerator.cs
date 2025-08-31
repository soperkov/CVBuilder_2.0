namespace CVBuilder.Api.Services
{
    public class PdfGenerator
    {
        private readonly ITemplateRenderService _render;
        private readonly IPhotoDataUriService _photoDataUri;

        public PdfGenerator(ITemplateRenderService render, IPhotoDataUriService photoDataUri)
        {
            _render = render;
            _photoDataUri = photoDataUri;
        }

        // Renders HTML for the CV, honoring the selected template.
        public async Task<string> RenderCVToHtmlAsync(CVModel model, string baseUrl, CancellationToken ct = default)
        {
            // shallow copy so we don't mutate EF-tracked entity
            var vm = new CVModel
            {
                Id = model.Id,
                CreatedByUser = model.CreatedByUser,
                User = model.User,
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
                Template = model.Template,
                PhotoUrl = model.PhotoUrl,
                Skills = model.Skills,
                Education = model.Education,
                Employment = model.Employment,
                Language = model.Language,
                CreatedAtUtc = model.CreatedAtUtc,
                UpdatedAtUtc = model.UpdatedAtUtc
            };

            // Inline photo as data: URI if you have one saved
            if (!string.IsNullOrWhiteSpace(model.PhotoUrl))
            {
                var absolute = $"{baseUrl}/api/uploads/photo?path={Uri.EscapeDataString(model.PhotoUrl)}";
                var dataUri = await _photoDataUri.ToDataUriAsync(absolute, ct);
                if (!string.IsNullOrWhiteSpace(dataUri))
                    vm.PhotoUrl = dataUri;
            }

            // Prefer the CV's own template name; fall back is handled inside the renderer
            var templateName = vm.Template?.Name;

            // Let the renderer find the correct component for the template name
            return await _render.RenderAsync(templateName, vm, ct);
        }
    }
}
