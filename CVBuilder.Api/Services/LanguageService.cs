
namespace CVBuilder.Api.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly AppDbContext _context;
        public LanguageService(AppDbContext context) 
        {
            _context = context;
        }
        public async Task<IReadOnlyList<LanguageDto>> GetAllAsync(string? search = null, CancellationToken ct = default)
        {
            var q = _context.Set<LanguageModel>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(l =>
                    l.Name.ToLower().Contains(s) ||
                    l.Code.ToLower().StartsWith(s));
            }

            return await q
                .OrderBy(l => l.Name)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name
                })
                .ToListAsync(ct);
        }

        public async Task<LanguageDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Set<LanguageModel>()
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
