namespace CVBuilder.Core.Interfaces
{
    public interface ITemplateService
    {
        Task<List<TemplateDto>> GetAllAsync(CancellationToken ct = default);
        Task<TemplateDto?> GetAsync(int id, CancellationToken ct = default);
        Task<string> RenderPreviewHtmlAsync(int id, CancellationToken ct = default);
    }
}
