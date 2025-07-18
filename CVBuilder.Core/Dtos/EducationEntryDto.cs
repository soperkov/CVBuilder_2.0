namespace CVBuilder.Core.Dtos
{
    public class EducationEntryDto
    {
        public int Id { get; set; }
        public string InstitutionName { get; set; }
        public string Description { get; set; }
        public DateTime From { get; set; }
        public DateTime? To { get; set; }
        public bool IsOngoing => To == null;
    }
}
