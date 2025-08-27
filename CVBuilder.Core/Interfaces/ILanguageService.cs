namespace CVBuilder.Core.Interfaces
{
    public interface ILanguageService
    {
        Task<IReadOnlyList<LanguageDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
        Task<LanguageDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
