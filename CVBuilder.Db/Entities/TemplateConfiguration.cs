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

            builder.HasIndex(t => t.Name)
                .IsUnique();

            builder.ToTable("Templates");
        }
    }
}