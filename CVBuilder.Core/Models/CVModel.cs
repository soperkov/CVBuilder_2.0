namespace CVBuilder.Core.Models
{
    public class CVModel
    {
        public int Id { get; set; }
        public int CreatedByUser { get; set; } 
        public UserModel User { get; set; }

        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? AboutMe { get; set; }
        public string? PhotoUrl { get; set; } 

        public List<SkillModel> Skills { get; set; }
        public List<EducationEntryModel>? Education { get; set; }
        public List<EmploymentEntryModel>? Employment { get; set; }

        public int? TemplateId { get; set; }
        public TemplateModel? Template { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
