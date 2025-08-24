namespace CVBuilder.Api.Services
{
    public static class TemplateSeeder
    {
        public static async Task SeedTemplatesAsync(
            AppDbContext db,
            ITemplateCatalog catalog,
            ILogger logger,
            CancellationToken ct = default)
        {
            var existing = await db.Templates.Select(t => t.Name).ToListAsync(ct);
            var toInsert = catalog.Names
                .Except(existing, StringComparer.OrdinalIgnoreCase)
                .Select(n => new TemplateModel { Name = n })
                .ToList();

            if (toInsert.Count == 0)
            {
                logger.LogInformation("TemplateSeeder: nothing to seed.");
                return;
            }

            db.Templates.AddRange(toInsert);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("TemplateSeeder: seeded {Count} templates: {Names}",
                toInsert.Count, string.Join(", ", toInsert.Select(x => x.Name)));
        }
    }
}
