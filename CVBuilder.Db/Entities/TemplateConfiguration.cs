namespace CVBuilder.Db.Entities
{
    public class TemplateConfiguration : IEntityTypeConfiguration<TemplateModel>
    {
        public void Configure(EntityTypeBuilder<TemplateModel> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.PreviewImageUrl)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.TemplateKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.HtmlTemplatePath)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.CssClass)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.FontFamily)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.PrimaryColor)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.AccentColor)
                .HasMaxLength(20);

            builder.Property(t => t.IsTwoColumn)
                .IsRequired();

            builder.Property(t => t.PaperSize)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.IsDefault)
                .IsRequired();

            builder.Property(t => t.IsActive)
                .IsRequired();

            builder.Property(t => t.Category)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            builder.ToTable("Templates");
        }
    }
}