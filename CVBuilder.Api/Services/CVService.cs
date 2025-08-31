namespace CVBuilder.Api.Services
{
    public class CVService : ICVService
    {
        private readonly AppDbContext _context;

        public CVService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateCvAsync(CreateCVDto dto, int userId, CancellationToken ct)
        {
            var cv = new CVModel
            {
                CVName = dto.CVName,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                AboutMe = dto.AboutMe,
                PhotoUrl = dto.PhotoUrl,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                CreatedByUser = userId,
                TemplateId = dto.TemplateId,
                Address = dto.Address,
                WebPage = dto.WebPage,
                JobTitle = dto.JobTitle,

                Skills = dto.Skills?.Select(s => new SkillModel { Name = s.Name }).ToList() ?? new List<SkillModel>(),
                Education = dto.Education?.Select(e =>
                {
                    DateTime? toDate = e.To;

                    if (!e.IsCurrent && toDate == null)
                    {
                        throw new ArgumentException("To date must be provided if the education entry is not current.");
                    }

                    return new EducationEntryModel
                    {
                        CVId = e.CVId,
                        InstitutionName = e.InstitutionName,
                        Description = e.Description,
                        From = e.From,
                        To = e.To,
                        IsCurrent = e.IsCurrent
                    };
                }).ToList() ?? new List<EducationEntryModel>(),

                Employment = dto.Employment?.Select(e =>
                {
                    DateTime? toDate = e.To;
                    if (!e.IsCurrent && toDate == null)
                    {
                        throw new ArgumentException("To date must be provided if the employment entry is not current.");
                    }

                    return new EmploymentEntryModel
                    {
                        CVId = e.CVId,
                        CompanyName = e.CompanyName,
                        Description = e.Description,
                        From = e.From,
                        To = e.To,
                        IsCurrent = e.IsCurrent
                    };
                }).ToList() ?? new List<EmploymentEntryModel>(),

                Language = dto.Language?.Select(l => new LanguageEntryModel
                {
                    CVId = l.CVId,
                    LanguageId = l.LanguageId,
                    Level = l.Level,
                }).ToList() ?? new List<LanguageEntryModel>()
            };

            if (dto.TemplateId.HasValue)
            {
                var templateExists = await _context.Templates.AnyAsync(t => t.Id == dto.TemplateId.Value, ct);
                if (!templateExists)
                    throw new ArgumentException("Template with specified ID does not exist.");
            }

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync(ct);
            return cv.Id;
        }

        public async Task<CVSummaryDto?> GetCvByIdAsync(int id, int userId, CancellationToken ct)
        {
            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .Include(c => c.Language)
                .Include(c => c.Template)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId, ct);

            return cv == null ? null : MapToDto(cv);
        }

        public async Task<List<CVSummaryDto>> GetMyCvsAsync(int userId, CancellationToken ct)
        {
            var cvs = await _context.CVs
                .Where(cv => cv.CreatedByUser == userId)
                .Include(cv => cv.Skills)
                .Include(cv => cv.Education)
                .Include(cv => cv.Employment)
                .Include(cv => cv.Language)
                .Include(cv => cv.Template)
                .ToListAsync(ct);

            return cvs.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateCvAsync(int id, CreateCVDto dto, int userId, CancellationToken ct)
        {
            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .Include(cv => cv.Language)
                .Include(c => c.Template)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId, ct);

            if (cv == null) return false;

            cv.CVName = dto.CVName;
            cv.FullName = dto.FullName;
            cv.DateOfBirth = dto.DateOfBirth;
            cv.PhoneNumber = dto.PhoneNumber;
            cv.Email = dto.Email;
            cv.AboutMe = dto.AboutMe;
            cv.PhotoUrl = dto.PhotoUrl;
            cv.TemplateId = dto.TemplateId;
            cv.Address = dto.Address;
            cv.WebPage = dto.WebPage;
            cv.JobTitle = dto.JobTitle;

            _context.Skills.RemoveRange(cv.Skills);
            cv.Skills = dto.Skills.Select(s => new SkillModel { Name = s.Name }).ToList();

            var eduIds = dto.Education.Where(e => e.Id != 0).Select(e => e.Id).ToHashSet();
            _context.Educations.RemoveRange(cv.Education.Where(e => !eduIds.Contains(e.Id)));

            foreach (var eduDto in dto.Education)
            {
                var existing = cv.Education.FirstOrDefault(e => e.Id == eduDto.Id);
                if (existing != null)
                {
                    existing.InstitutionName = eduDto.InstitutionName;
                    existing.Description = eduDto.Description;
                    existing.From = eduDto.From;
                    existing.To = eduDto.To;
                    existing.CVId = cv.Id;
                    existing.IsCurrent = eduDto.IsCurrent;
                }
                else
                {
                    cv.Education.Add(new EducationEntryModel
                    {
                        InstitutionName = eduDto.InstitutionName,
                        Description = eduDto.Description,
                        From = eduDto.From,
                        To = eduDto.To,
                        CVId = cv.Id,
                        IsCurrent = eduDto.IsCurrent
                    });
                }
            }

            var empIds = dto.Employment.Where(e => e.Id != 0).Select(e => e.Id).ToHashSet();
            _context.Employments.RemoveRange(cv.Employment.Where(e => !empIds.Contains(e.Id)));

            foreach (var empDto in dto.Employment)
            {
                var existing = cv.Employment.FirstOrDefault(e => e.Id == empDto.Id);
                if (existing != null)
                {
                    existing.CompanyName = empDto.CompanyName;
                    existing.Description = empDto.Description;
                    existing.From = empDto.From;
                    existing.To = empDto.To;
                    existing.CVId = cv.Id;
                    existing.IsCurrent = empDto.IsCurrent;
                }
                else
                {
                    cv.Employment.Add(new EmploymentEntryModel
                    {
                        CompanyName = empDto.CompanyName,
                        Description = empDto.Description,
                        From = empDto.From,
                        To = empDto.To,
                        CVId = cv.Id,
                        IsCurrent = empDto.IsCurrent
                    });
                }
            }

            var langIds = dto.Language.Where(l => l.Id != 0).Select(l => l.Id).ToHashSet();
            _context.LanguageEntries.RemoveRange(cv.Language.Where(l => !langIds.Contains(l.Id)));
            foreach (var langDto in dto.Language)
            {
                var existing = cv.Language.FirstOrDefault(l => l.Id == langDto.Id);
                if (existing != null)
                {
                    existing.LanguageId = langDto.LanguageId;
                    existing.Level = langDto.Level;
                    existing.CVId = cv.Id;
                }
                else
                {
                    cv.Language.Add(new LanguageEntryModel
                    {
                        LanguageId = langDto.LanguageId,
                        Level = langDto.Level,
                        CVId = cv.Id
                    });
                }
            }

            cv.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteCvAsync(int id, int userId, CancellationToken ct)
        {
            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .Include(c => c.Language)
                .Include(c => c.Template)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId, ct);

            if (cv == null) return false;

            _context.Skills.RemoveRange(cv.Skills);
            _context.Educations.RemoveRange(cv.Education);
            _context.Employments.RemoveRange(cv.Employment);
            _context.LanguageEntries.RemoveRange(cv.Language);
            _context.CVs.Remove(cv);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var idSet = ids?.Distinct().ToList() ?? new List<int>();
            if (idSet.Count == 0) return;

            var stubs = idSet.Select(id => new CVModel { Id = id }).ToList();
            _context.AttachRange(stubs);
            _context.RemoveRange(stubs);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<string?> GetPhotoUrl(int id, int userId, CancellationToken ct = default)
        {
            return await _context.CVs
                .Where(c => c.Id == id && c.CreatedByUser == userId)
                .Select(c => c.PhotoUrl)
                .FirstOrDefaultAsync(ct);
        }

        private static CVSummaryDto MapToDto(CVModel cv) => new()
        {
            Id = cv.Id,
            CVName = cv.CVName,
            FullName = cv.FullName,
            DateOfBirth = cv.DateOfBirth,
            PhoneNumber = cv.PhoneNumber,
            Email = cv.Email,
            AboutMe = cv.AboutMe,
            PhotoUrl = cv.PhotoUrl,
            Address = cv.Address,
            WebPage = cv.WebPage,
            JobTitle = cv.JobTitle,
            CreatedAt = cv.CreatedAtUtc,
            UpdatedAt = cv.UpdatedAtUtc,
            TemplateId = cv.TemplateId,
            TemplateName = cv.Template?.Name ?? "(unknown)",
            Skills = cv.Skills.Select(s => s.Name).ToList(),
            Education = cv.Education.Select(e => new EducationEntryDto
            {
                Id = e.Id,
                InstitutionName = e.InstitutionName,
                Description = e.Description,
                From = e.From,
                To = e.To,
                IsCurrent = e.IsCurrent,
                CVId = e.CVId
            }).ToList(),
            Employment = cv.Employment.Select(e => new EmploymentEntryDto
            {
                Id = e.Id,
                CompanyName = e.CompanyName,
                Description = e.Description,
                From = e.From,
                To = e.To,
                IsCurrent = e.IsCurrent,
                CVId = e.CVId
            }).ToList(),
            Language = cv.Language.Select(l => new LanguageEntryDto
            {
                Id = l.Id,
                LanguageId = l.LanguageId,
                Level = l.Level,
                CVId = l.CVId
            }).ToList()
        };

        public async Task<CVModel?> GetCvForRenderAsync(int id, int userId, CancellationToken ct)
        {
            return await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .Include(c => c.Language)
                    .ThenInclude(l => l.Language)
                .Include(c => c.Template)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId, ct);
        }

        public async Task SetPhotoAsync(int id, int userId, string relativePath, CancellationToken ct)
        {
            var cv = await _context.CVs
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId, ct);

            if (cv == null)
                throw new KeyNotFoundException("CV not found or not yours.");

            cv.PhotoUrl = relativePath;
            cv.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }
}