namespace CVBuilder.Core.Interfaces
{
    public interface IPlaywrightPdfService
    {
        Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(int cvId, int userId, CancellationToken ct = default);

        Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(int cvId, int userId, string baseUrl, CancellationToken ct = default);
    }
}
