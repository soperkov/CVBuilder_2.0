
namespace CVBuilder.Db.Entities
{
    public class LanguageConfiguration : IEntityTypeConfiguration<LanguageModel>
    {
        public void Configure(EntityTypeBuilder<LanguageModel> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Code)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.ToTable("Languages");
        }
    }
}
