namespace CVBuilder.Api.Services
{
    public class PhotoDataUriService : IPhotoDataUriService
    {
        private readonly IHttpClientFactory _http;
        private readonly IWebHostEnvironment _env;

        public PhotoDataUriService(IHttpClientFactory http, IWebHostEnvironment env)
        {
            _http = http; _env = env;
        }

        public async Task<string?> ToDataUriAsync(string? src, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(src)) return null;

            if (Uri.TryCreate(src, UriKind.Absolute, out var uri))
            {
                // DEV-ONLY: for HTTPS, use a handler that accepts any cert.
                // This bypasses all TLS validation so the handshake succeeds.
                HttpClient client;
                if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    var handler = new HttpClientHandler
                    {
                        SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator, // <— DEV ONLY
                        AllowAutoRedirect = true
                    };
                    client = new HttpClient(handler, disposeHandler: true)
                    {
                        Timeout = TimeSpan.FromSeconds(20)
                    };
                }
                else
                {
                    // Plain HTTP (or other) can use your factory/default client
                    client = _http.CreateClient();
                }

                using (client)
                using (var resp = await client.GetAsync(uri, ct))
                {
                    resp.EnsureSuccessStatusCode();
                    var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
                    var mime = resp.Content.Headers.ContentType?.MediaType ?? GuessMimeFromPath(uri.AbsolutePath);
                    return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
                }
            }

            // local relative path in App_Data (unchanged)
            var relative = src.Replace('\\', '/').TrimStart('/');
            var fullPath = Path.Combine(_env.ContentRootPath, "App_Data", relative);
            if (!File.Exists(fullPath)) return null;

            var bytesLocal = await File.ReadAllBytesAsync(fullPath, ct);
            var mimeLocal = GuessMimeFromPath(fullPath);
            return $"data:{mimeLocal};base64,{Convert.ToBase64String(bytesLocal)}";
        }

        private static string GuessMimeFromPath(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" or ".tif" => "image/tiff",
                ".svg" => "image/svg+xml",
                ".jfif" => "image/jfif",
                _ => "application/octet-stream"
            };
        }
    }
}
