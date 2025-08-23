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

            builder.Property(t => t.CssContent)
                   .IsRequired()
                   .HasColumnType("nvarchar(max)");

            builder.Property(t => t.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.ToTable("Templates");
        }
    }
}