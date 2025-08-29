namespace CVBuilder.Core.Interfaces
{
    public interface ITemplateService
    {
        Task<List<TemplateModel>> GetAllAsync(CancellationToken ct = default);
        Task<TemplateModel?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
