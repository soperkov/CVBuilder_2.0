
namespace CVBuilder.Db.Entities
{
    public class EmploymentEntryConfiguration : IEntityTypeConfiguration<EmploymentEntryModel>
    {
        public void Configure(EntityTypeBuilder<EmploymentEntryModel> builder)
        {
            builder.HasKey(e  => e.Id);
            builder.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(e => e.From)
               .IsRequired();
            builder.Property(e => e.To)
                .IsRequired(false);
            builder.HasOne<CVModel>()
                .WithMany(c => c.Employment)
                .HasForeignKey(e => e.CVId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.ToTable("Employments");
        }
    }
}
