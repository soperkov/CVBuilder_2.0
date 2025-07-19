namespace CVBuilder.Db.Entities
{
    public class EducationEntryConfiguration : IEntityTypeConfiguration<EducationEntryModel>
    {
        public void Configure(EntityTypeBuilder<EducationEntryModel> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.InstitutionName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(e => e.Description)
                .HasMaxLength(500);
            builder.Property(e => e.From)
                .IsRequired();
            builder.Property(e => e.To)
                .IsRequired(false);
            builder.HasOne<CVModel>()
                .WithMany(c => c.Education)
                .HasForeignKey(e => e.CVId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable("EducationEntries");
        }
    }
}
