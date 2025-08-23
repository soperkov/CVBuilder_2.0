namespace CVBuilder.Core.Models
{
    public class TemplateModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }

        public string CssContent { get; set; } = ""; 

        public bool IsActive { get; set; } = true;
    }
}
