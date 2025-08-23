namespace CVBuilder.Core.Dtos
{

    public class TemplateDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }

        public bool IsActive { get; set; }

    }
}
