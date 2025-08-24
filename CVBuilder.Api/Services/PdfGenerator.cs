
using Microsoft.AspNetCore.Components.RenderTree;

namespace CVBuilder.Api.Services
{
    public class PdfGenerator
    {
        private IServiceCollection _services = new ServiceCollection();

        public PdfGenerator(IServiceCollection services)
        {
            _services = services;
            _services.AddLogging();
        }

        public async Task<string> RenderCVToHtmlAsync(CVModel model)
        {
            var serviceProvider = _services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            
            await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

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
