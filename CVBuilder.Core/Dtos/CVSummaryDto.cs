namespace CVBuilder.Core.Dtos
{
    public class CVSummaryDto
    {
        public int Id { get; set; }
        public string CVName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? AboutMe { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Address { get; set; }
        public string? WebPage { get; set; }
        public string? JobTitle { get; set; }

        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<string> Skills { get; set; }
        public List<EducationEntryDto>? Education { get; set; }
        public List<EmploymentEntryDto>? Employment { get; set; }
        public List<LanguageEntryDto>? Language { get; set; }
    }
}
