namespace CVBuilder.Core.Interfaces
{
    public interface ITemplateRenderService
    {
        Task<string> RenderAsync(string? templateName, CVModel model, CancellationToken ct = default);
    }
}
