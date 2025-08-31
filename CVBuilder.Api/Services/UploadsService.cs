public class UploadsService : IUploadsService
{
    private readonly IWebHostEnvironment _env;
    private string StorageRoot => Path.Combine(_env.ContentRootPath, "App_Data");

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".tiff", ".jfif" };
    private const long MaxBytes = 5 * 1024 * 1024;

    public UploadsService(IWebHostEnvironment env) => _env = env;

    public async Task<UploadResult> SavePrivatePhotoAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0) throw new ArgumentException("No file provided.");
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Only images allowed.");
        if (file.Length > MaxBytes) throw new InvalidOperationException("File too large.");
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext)) throw new InvalidOperationException("Invalid image type.");

        var relativeDir = Path.Combine("uploads", "photos");
        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var relativePath = Path.Combine(relativeDir, fileName);
        Directory.CreateDirectory(Path.Combine(StorageRoot, relativeDir));

        var fullPath = Path.Combine(StorageRoot, relativePath);
        await using (var fs = File.Create(fullPath))
            await file.CopyToAsync(fs, ct);

        return new UploadResult(relativePath.Replace('\\', '/'), fileName, file.Length, file.ContentType);
    }

    public async Task<FileReadResult?> TryOpenPhotoAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        relativePath = relativePath.Replace('\\', '/').TrimStart('/');
        if (relativePath.Contains("..", StringComparison.Ordinal)) return null;

        var fullPath = Path.Combine(StorageRoot, relativePath);
        if (!File.Exists(fullPath)) return null;

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = GetContentTypeFromExt(Path.GetExtension(fullPath));
        return await Task.FromResult(new FileReadResult { Stream = stream, ContentType = contentType });
    }

    private static string GetContentTypeFromExt(string ext) => (ext.ToLowerInvariant()) switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "application/octet-stream"
    };
}
