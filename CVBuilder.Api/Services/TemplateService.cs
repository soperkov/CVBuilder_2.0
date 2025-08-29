
namespace CVBuilder.Api.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly AppDbContext _context;

        public TemplateService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<TemplateModel>> GetAllAsync(CancellationToken ct = default)
        {
            return _context.Templates
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(ct);
        }

        public Task<TemplateModel?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.Templates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }
    }
}
