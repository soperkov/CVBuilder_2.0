
using Microsoft.AspNetCore.Components.RenderTree;

namespace CVBuilder.Api.Services
{
    public class PdfGenerator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IPhotoDataUriService _photoDataUri;

        public PdfGenerator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IPhotoDataUriService photoDataUri)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _photoDataUri = photoDataUri;
        }

        public async Task<string> RenderCVToHtmlAsync(CVModel model, CancellationToken ct = default)
        {
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
                Skills = model.Skills,
                Education = model.Education,
                Employment = model.Employment,
                Language = model.Language,
                Template = model.Template
            };

            var dataUri = await _photoDataUri.ToDataUriAsync(model.PhotoUrl, ct);
            if (!string.IsNullOrWhiteSpace(dataUri))
                vm.PhotoUrl = dataUri;

            await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);
            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = vm });
                var result = await htmlRenderer.RenderComponentAsync<Templates.CvClassic>(parameters);
                return result.ToHtmlString();
            });

            return html;
        }
    }
}
