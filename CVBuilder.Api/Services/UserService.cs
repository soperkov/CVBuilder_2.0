namespace CVBuilder.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserInfoDto?> GetMeAsync(ClaimsPrincipal user)
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;

            var result = await _context.Users
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Skills)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Education)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Employment)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Template)
                .Where(u => u.Id == userId)
                .Select(u => new UserInfoDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Cvs = u.CVs.Select(cv => new CVSummaryDto
                    {
                        Id = cv.Id,
                        FullName = cv.FullName,
                        DateOfBirth = cv.DateOfBirth,
                        PhoneNumber = cv.PhoneNumber,
                        Email = cv.Email,
                        AboutMe = cv.AboutMe,
                        PhotoUrl = cv.PhotoUrl,
                        CreatedAt = cv.CreatedAtUtc,
                        TemplateName = cv.Template != null ? cv.Template.Name : "(unknown)",
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
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
