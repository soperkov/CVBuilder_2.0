namespace CVBuilder.Core.Models
{
    public class TemplateModel
    {
        public int Id { get; set; }

        // Display & Identity
        public string Name { get; set; }                 
        public string? Description { get; set; }           
        public string PreviewImageUrl { get; set; }      
        public string TemplateKey { get; set; }           

        // Rendering Configuration
        public string HtmlTemplatePath { get; set; }       
        public string CssClass { get; set; }             
        public string FontFamily { get; set; }         
        public string PrimaryColor { get; set; }        
        public string? AccentColor { get; set; }          

        // Layout Options
        public bool IsTwoColumn { get; set; }           
        public string PaperSize { get; set; }      

        // Usage Flags
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        // Optional metadata
        public string Category { get; set; }    
        public string? Notes { get; set; }
    }
}
