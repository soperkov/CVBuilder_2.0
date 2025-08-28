namespace CVBuilder.Core.Interfaces
{
    public interface IPhotoDataUriService
    {
        Task<string?> ToDataUriAsync(string? urlOrRelativePath, CancellationToken ct = default);
    }
}
