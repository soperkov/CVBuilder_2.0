
using Microsoft.AspNetCore.Components.RenderTree;

namespace CVBuilder.Api.Services
{
    public class PdfGenerator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;

        public PdfGenerator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }

        public async Task<string> RenderCVToHtmlAsync(CVModel model)
        {            
            await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);

            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["Model"] = model
                });

                var result = await htmlRenderer.RenderComponentAsync<Templates.CvClassic>(parameters);
                return result.ToHtmlString();
            });

            return html;
        }
    }
}
