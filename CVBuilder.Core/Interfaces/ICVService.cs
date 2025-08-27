namespace CVBuilder.Core.Interfaces
{
    public interface ICVService
    {
        Task<int> CreateCvAsync(CreateCVDto dto, int userId);
        Task<CVSummaryDto?> GetCvByIdAsync(int id, int userId);
        Task<List<CVSummaryDto>> GetMyCvsAsync(int userId);
        Task<bool> UpdateCvAsync(int id, CreateCVDto dto, int userId);
        Task<bool> DeleteCvAsync(int id, int userId);
        Task DeleteManyAsync(IEnumerable<int> ids);

        Task<CVModel?> GetCvForRenderAsync(int id, int userId);
        Task<string?> GetPhotoUrl(int id, int userId, CancellationToken ct = default);
        Task SetPhotoAsync(int id, int userId, string relativePath, CancellationToken ct = default);
    }
}
