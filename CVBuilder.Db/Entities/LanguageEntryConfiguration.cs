namespace CVBuilder.Db.Entities
{
    public class LanguageEntryConfiguration : IEntityTypeConfiguration<LanguageEntryModel>
    {
        public void Configure(EntityTypeBuilder<LanguageEntryModel> builder)
        {
            builder.HasKey(le => le.Id);

            builder.HasOne<CVModel>()
                   .WithMany(c => c.Language)
                   .HasForeignKey(le => le.CVId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<LanguageModel>()
                   .WithMany()
                   .HasForeignKey(le => le.LanguageId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(le => le.Language)    
               .WithMany()
               .HasForeignKey(le => le.LanguageId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(le => le.Level)
                   .IsRequired();

            builder.ToTable("LanguageEntries");
        }
    }
}
