namespace CVBuilder.Core.Interfaces
{
    public interface ICVService
    {
        Task<int> CreateCvAsync(CreateCVDto dto, int userId);
        Task<CVSummaryDto?> GetCvByIdAsync(int id, int userId);
        Task<List<CVSummaryDto>> GetMyCvsAsync(int userId);
        Task<bool> UpdateCvAsync(int id, CreateCVDto dto, int userId);
        Task<bool> DeleteCvAsync(int id, int userId);
    }
}
