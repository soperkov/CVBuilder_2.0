namespace CVBuilder.Core.Interfaces
{
    public interface IPlaywrightPdfService
    {
        Task<(byte[] Bytes, string FileName)> GenerateByCvIdAsync(int cvId, int userId, CancellationToken ct = default);
    }
}
