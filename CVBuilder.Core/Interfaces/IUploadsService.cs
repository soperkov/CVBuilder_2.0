namespace CVBuilder.Core.Interfaces
{
    public interface IUploadsService
    {
        Task<UploadResult> SavePrivatePhotoAsync(IFormFile file, CancellationToken ct = default);
        Task<FileReadResult?> TryOpenPhotoAsync(string relativePath, CancellationToken ct = default);
    }

    public sealed record UploadResult(string Path, string FileName, long Size, string ContentType);

    public sealed class FileReadResult : IDisposable
    {
        public required Stream Stream { get; init; }
        public required string ContentType { get; init; }
        public void Dispose() => Stream.Dispose();
    }
}
