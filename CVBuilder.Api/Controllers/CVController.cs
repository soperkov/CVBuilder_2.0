using CVBuilder.Core.Dtos;

namespace CVBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CVController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCV([FromBody] CreateCVDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var cv = new CVModel
            {
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                AboutMe = dto.AboutMe,
                PhotoUrl = dto.PhotoUrl,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUser = userId,
                TemplateId = dto.TemplateId,

                Skills = dto.Skills?.Select(s => new SkillModel
                {
                    Name = s.Name
                }).ToList() ?? new List<SkillModel>(),

                Education = dto.Education?.Select(e => new EducationEntryModel
                {
                    InstitutionName = e.InstitutionName,
                    Description = e.Description,
                    From = e.From,
                    To = e.To 
                }).ToList() ?? new List<EducationEntryModel>(),

                Employment = dto.Employment?.Select(e => new EmploymentEntryModel
                {
                    CompanyName = e.CompanyName,
                    Description = e.Description,
                    From = e.From,
                    To = e.To 
                }).ToList() ?? new List<EmploymentEntryModel>()
            };

            if (dto.TemplateId.HasValue)
            {
                var templateExists = await _context.Templates.AnyAsync(t => t.Id == dto.TemplateId.Value);
                if (!templateExists)
                {
                    return BadRequest("Template with specified ID does not exist.");
                }
            }

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            return Ok(new { cv.Id });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CVSummaryDto>> GetCvById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .Include(c => c.Template)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId);

            if (cv == null)
                return NotFound();

            var dto = new CVSummaryDto
            {
                Id = cv.Id,
                FullName = cv.FullName,
                DateOfBirth = cv.DateOfBirth,
                PhoneNumber = cv.PhoneNumber,
                Email = cv.Email,
                AboutMe = cv.AboutMe,
                PhotoUrl = cv.PhotoUrl,
                CreatedAt = cv.CreatedAtUtc,
                TemplateName = cv.Template?.Name ?? "(unknown)",
                Skills = cv.Skills.Select(s => s.Name).ToList(),
                Education = cv.Education.Select(e => new EducationEntryDto
                {
                    Id = e.Id,
                    InstitutionName = e.InstitutionName,
                    Description = e.Description,
                    From = e.From,
                    To = e.To
                }).ToList(),
                Employment = cv.Employment.Select(e => new EmploymentEntryDto
                {
                    Id = e.Id,
                    CompanyName = e.CompanyName,
                    Description = e.Description,
                    From = e.From,
                    To = e.To
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<CVSummaryDto>>> GetMyCVs()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var cvs = await _context.CVs
                .Where(cv => cv.CreatedByUser == userId)
                .Include(cv => cv.Skills)
                .Include(cv => cv.Education)
                .Include(cv => cv.Employment)
                .Include(cv => cv.Template)
                .ToListAsync();

            var result = cvs.Select(cv => new CVSummaryDto
            {
                Id = cv.Id,
                FullName = cv.FullName,
                DateOfBirth = cv.DateOfBirth,
                PhoneNumber = cv.PhoneNumber,
                Email = cv.Email,
                AboutMe = cv.AboutMe,
                PhotoUrl = cv.PhotoUrl,
                CreatedAt = cv.CreatedAtUtc,
                TemplateName = cv.Template?.Name ?? "(unknown)",
                Skills = cv.Skills.Select(s => s.Name).ToList(),
                Education = cv.Education.Select(e => new EducationEntryDto
                {
                    Id = e.Id,
                    InstitutionName = e.InstitutionName,
                    Description = e.Description,
                    From = e.From,
                    To = e.To
                }).ToList(),
                Employment = cv.Employment.Select(emp => new EmploymentEntryDto
                {
                    Id = emp.Id,
                    CompanyName = emp.CompanyName,
                    Description = emp.Description,
                    From = emp.From,
                    To = emp.To
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCv(int id, [FromBody] CreateCVDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var cv = await _context.CVs
                .Include(c => c.Skills)
                .Include(c => c.Education)
                .Include(c => c.Employment)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUser == userId);

            if (cv == null)
                return NotFound();

            cv.FullName = dto.FullName;
            cv.DateOfBirth = dto.DateOfBirth;
            cv.PhoneNumber = dto.PhoneNumber;
            cv.Email = dto.Email;
            cv.AboutMe = dto.AboutMe;
            cv.PhotoUrl = dto.PhotoUrl;
            cv.TemplateId = dto.TemplateId;

            _context.Skills.RemoveRange(cv.Skills);
            cv.Skills = dto.Skills.Select(s => new SkillModel { Name = s.Name }).ToList();

            var incomingEducationIds = dto.Education.Where(e => e.Id != 0).Select(e => e.Id).ToHashSet();
            var educationToRemove = cv.Education.Where(e => !incomingEducationIds.Contains(e.Id)).ToList();
            _context.Educations.RemoveRange(educationToRemove);

            foreach (var eduDto in dto.Education)
            {
                var existing = cv.Education.FirstOrDefault(e => e.Id == eduDto.Id);
                if (existing != null)
                {
                    existing.InstitutionName = eduDto.InstitutionName;
                    existing.Description = eduDto.Description;
                    existing.From = eduDto.From;
                    existing.To = eduDto.To;
                }
                else
                {
                    cv.Education.Add(new EducationEntryModel
                    {
                        InstitutionName = eduDto.InstitutionName,
                        Description = eduDto.Description,
                        From = eduDto.From,
                        To = eduDto.To,
                        CVId = cv.Id
                    });
                }
            }

            var incomingEmploymentIds = dto.Employment.Where(e => e.Id != 0).Select(e => e.Id).ToHashSet();
            var employmentToRemove = cv.Employment.Where(e => !incomingEmploymentIds.Contains(e.Id)).ToList();
            _context.Employments.RemoveRange(employmentToRemove);

            foreach (var empDto in dto.Employment)
            {
                var existing = cv.Employment.FirstOrDefault(e => e.Id == empDto.Id);
                if (existing != null)
                {
                    existing.CompanyName = empDto.CompanyName;
                    existing.Description = empDto.Description;
                    existing.From = empDto.From;
                    existing.To = empDto.To;
                }
                else
                {
                    cv.Employment.Add(new EmploymentEntryModel
                    {
                        CompanyName = empDto.CompanyName,
                        Description = empDto.Description,
                        From = empDto.From,
                        To = empDto.To,
                        CVId = cv.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
