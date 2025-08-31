namespace CVBuilder.Core.Interfaces
{
    public interface ICVService
    {
        Task<int> CreateCvAsync(CreateCVDto dto, int userId, CancellationToken ct = default!);
        Task<CVSummaryDto?> GetCvByIdAsync(int id, int userId, CancellationToken ct = default!);
        Task<List<CVSummaryDto>> GetMyCvsAsync(int userId, CancellationToken ct = default!);
        Task<bool> UpdateCvAsync(int id, CreateCVDto dto, int userId, CancellationToken ct = default!);
        Task<bool> DeleteCvAsync(int id, int userId, CancellationToken ct = default!);
        Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken ct = default!);

        Task<CVModel?> GetCvForRenderAsync(int id, int userId, CancellationToken ct = default!);
        Task<string?> GetPhotoUrl(int id, int userId, CancellationToken ct = default);
        Task SetPhotoAsync(int id, int userId, string relativePath, CancellationToken ct = default);
    }
}
