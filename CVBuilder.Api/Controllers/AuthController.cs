namespace CVBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, JwtService jwtService, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email == dto.Email))
            {
                return BadRequest(new
                {
                    title = "Email is already registered."
                });
            }


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

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserInfoDto>> Me()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Skills)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Education)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Employment)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Template)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

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
                    Education = cv.Education?.Select(e => new EducationEntryDto
                    {
                        Id = e.Id,
                        InstitutionName = e.InstitutionName,
                        Description = e.Description,
                        From = e.From,
                        To = e.To
                    }).ToList() ?? new List<EducationEntryDto>(),
                    Employment = cv.Employment?.Select(emp => new EmploymentEntryDto
                    {
                        Id = emp.Id,
                        CompanyName = emp.CompanyName,
                        Description = emp.Description,
                        From = emp.From,
                        To = emp.To
                    }).ToList() ?? new List<EmploymentEntryDto>()
                }).ToList()
            };
        }

        // not working for now
        //[HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        //    if (user == null)
        //    {
        //        Console.WriteLine("User not found, returning OK anyway.");
        //        return Ok(); // Avoid leaking info
        //    }

        //    var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        //    user.ResetToken = token;
        //    user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
        //    await _context.SaveChangesAsync();

        //    var resetLink = $"http://localhost:4200/reset-password?token={token}&email={dto.Email}";
        //    Console.WriteLine($"Reset link: {resetLink}");

        //    await _emailService.SendResetPasswordEmail(dto.Email, resetLink);
        //    return Ok(); /*$"https://placeholder.com/reset-password?token={token}&email={dto.Email}"*/
        //}

        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u =>
        //        u.Email == dto.Email &&
        //        u.ResetToken == dto.Token &&
        //        u.ResetTokenExpires > DateTime.UtcNow);

        //    if (user == null) return BadRequest("Invalid or expired token.");

        //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        //    user.ResetToken = null;
        //    user.ResetTokenExpires = null;

        //    await _context.SaveChangesAsync();

        //    return Ok("Password successfully reset.");
        //}
    }
}