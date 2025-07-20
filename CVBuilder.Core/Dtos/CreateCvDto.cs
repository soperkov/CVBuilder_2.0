namespace CVBuilder.Core.Dtos
{
    public class CreateCvDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? AboutMe { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }
        public int? TemplateId { get; set; }
    }
}
