namespace CVBuilder.Core.Dtos
{
    public class LanguageEntryDto
    {
        public int Id { get; set; }
        public int CVId { get; set; }
        public int LanguageId { get; set; }
        public LanguageLevel Level { get; set; }
    }
}
