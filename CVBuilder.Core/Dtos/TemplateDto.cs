namespace CVBuilder.Core.Dtos
{

    public class TemplateDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

    }

    public sealed class TemplateOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string ThumbUrl { get; set; } = default!;
    }
}
