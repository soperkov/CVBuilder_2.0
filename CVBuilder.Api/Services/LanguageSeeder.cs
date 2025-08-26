using System.Text.Json;

namespace CVBuilder.Api.Services
{
    public static class LanguageSeeder
    {
        public static async Task SeedLanguagesAsync(IServiceProvider services, ILogger logger)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var canConnect = await db.Database.CanConnectAsync();
            if (!canConnect) { logger.LogWarning("Database not ready, skipping language seed."); return; }

            var dbAssembly = typeof(AppDbContext).Assembly;

            var resourceName = "CVBuilder.Db.SeedData.languages.json";
            await using var stream = dbAssembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var rows = JsonSerializer.Deserialize<List<LangRow>>(json) ?? new();

            var existing = await db.Set<LanguageModel>()
                .AsTracking()
                .ToDictionaryAsync(x => x.Code.ToLower());

            int added = 0, updated = 0;

            foreach (var r in rows)
            {
                var code = (r.code ?? "").Trim().ToLower();
                var name = (r.name ?? "").Trim();

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                    continue;

                if (!existing.TryGetValue(code, out var entity))
                {
                    db.Add(new LanguageModel { Code = code, Name = name });
                    added++;
                }
                else if (!string.Equals(entity.Name, name, StringComparison.Ordinal))
                {
                    entity.Name = name;
                    updated++;
                }
            }

            if (added + updated > 0)
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded languages: added={Added}, updated={Updated}.", added, updated);
            }
            else
            {
                logger.LogInformation("Languages already up-to-date. No changes.");
            }
        }

        private sealed class LangRow
        {
            public string? code { get; set; }
            public string? name { get; set; }
        }

    }
}
