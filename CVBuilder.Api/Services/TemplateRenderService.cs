namespace CVBuilder.Api.Services
{
    public class TemplateRenderService : ITemplateRenderService
    {
        private readonly IServiceProvider _sp;
        private readonly ILoggerFactory _lf;
        private readonly ITemplateCatalog _catalog;

        public TemplateRenderService(IServiceProvider sp, ILoggerFactory lf, ITemplateCatalog catalog)
        {
            _sp = sp;
            _lf = lf;
            _catalog = catalog;
        }

        public async Task<string> RenderAsync(string? templateName, CVModel model, CancellationToken ct = default)
        {
            var componentType =
                (templateName != null && _catalog.TryGet(templateName, out var t)) ? t
                : _catalog.All.Values.FirstOrDefault()
                  ?? throw new InvalidOperationException("No template components discovered.");

            await using var renderer = new HtmlRenderer(_sp, _lf);

            return await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var pv = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = model });
                var result = await renderer.RenderComponentAsync(componentType, pv);
                return result.ToHtmlString();
            });
        }
    }
}
