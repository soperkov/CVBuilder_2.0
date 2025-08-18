namespace CVBuilder.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<string?> RegisterAsync(RegisterUserDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return null;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new UserModel
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _jwtService.GenerateToken(user);
        }

        public async Task<string?> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return _jwtService.GenerateToken(user);
        }

        public async Task<UserInfoDto?> GetUserInfoAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.CVs).ThenInclude(cv => cv.Skills)
                .Include(u => u.CVs).ThenInclude(cv => cv.Education)
                .Include(u => u.CVs).ThenInclude(cv => cv.Employment)
                .Include(u => u.CVs).ThenInclude(cv => cv.Template)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Cvs = user.CVs.Select(cv => new CVSummaryDto
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
                }).ToList()
            };
        }
    }

}
