namespace CVBuilder.Core.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateCvPdfAsync(int cvId, int userId, CancellationToken ct = default);
    }
}
