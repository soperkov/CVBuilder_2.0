namespace CVBuilder.Api.Services
{
    public sealed class DummyCvFactory : IDummyCvFactory
    {
        private readonly IWebHostEnvironment _env;
        private string? _cachedPhotoDataUri;

        public DummyCvFactory(IWebHostEnvironment env)
        {
            _env = env;
        }
        public CVModel Create()
        {
            return new CVModel
            {
                CVName = "Sample",
                FullName = "Jane Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                PhoneNumber = "+1 555-123-4567",
                AboutMe = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor nincididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud nexercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                WebPage = "https://janedoe.com",
                Address = "123 Main St, Anytown, USA",
                PhotoUrl = GetDummyPhotoDataUri(),
                JobTitle = "Senior UX Designer",
                Skills = new List<SkillModel>
                {
                    new() { Name = "Figma" },
                    new() { Name = "Design Systems" },
                    new() { Name = "Accessibility" },
                    new() { Name = "Prototyping" }
                },
                Education = new List<EducationEntryModel>
                {
                    new()
                    {
                        InstitutionName = "State University",
                        From = new DateTime(2008, 9, 1),
                        To = new DateTime(2012, 6, 1),
                        Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor nincididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud nexercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
                    }
                },
                Employment = new List<EmploymentEntryModel>
                {
                    new()
                    {
                        CompanyName = "Tech Solutions Inc.",
                        From = new DateTime(2020, 1, 1),
                        To = null,
                        Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor nincididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud nexercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
                    },
                },
                Language = new List<LanguageEntryModel>
                {
                    new() { LanguageId = 8, Level = Core.Enums.LanguageLevel.B2 },
                    new() { LanguageId = 44, Level = Core.Enums.LanguageLevel.C1 },
                    new() { LanguageId = 14, Level = Core.Enums.LanguageLevel.A2 }
                }
            };
        }

        private string GetDummyPhotoDataUri()
        {
            if (_cachedPhotoDataUri is not null)
                return _cachedPhotoDataUri;

            try
            {
                var path = Path.Combine(_env.WebRootPath ?? "", "dummy", "janedoe.png");
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    var b64 = Convert.ToBase64String(bytes);
                    _cachedPhotoDataUri = $"data:image/png;base64,{b64}";
                    return _cachedPhotoDataUri;
                }
            }
            catch
            {
                // fall through to transparent pixel
            }

            // 1x1 transparent PNG as a safe fallback
            _cachedPhotoDataUri =
                "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFdQJaDN3H3gAAAABJRU5ErkJggg==";
            return _cachedPhotoDataUri;
        }
    }
}
