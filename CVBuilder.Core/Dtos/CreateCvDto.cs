namespace CVBuilder.Core.Dtos
{
    public class CreateCVDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? AboutMe { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }

        public int? TemplateId { get; set; }

        public List<SkillDto> Skills { get; set; } = new();
        public List<EducationEntryDto> Education { get; set; } = new();
        public List<EmploymentEntryDto> Employment { get; set; } = new();
    }
}
