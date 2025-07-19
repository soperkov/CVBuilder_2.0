namespace CVBuilder.Db.Entities
{
    public class CVConfiguration : IEntityTypeConfiguration<CVModel>
    {
        public void Configure(EntityTypeBuilder<CVModel> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.FullName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(15);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(100);
            builder.Property(c => c.AboutMe).HasMaxLength(500);
            builder.Property(c => c.PhotoUrl).HasMaxLength(200);
            builder.HasOne(c => c.User)
                   .WithMany(u => u.CVs)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Skills)
                   .WithOne()
                   .HasForeignKey(s => s.CVId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Education)
                   .WithOne()
                   .HasForeignKey(e => e.CVId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Employment)
                   .WithOne()
                   .HasForeignKey(e => e.CVId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(c => c.Template)
                   .WithMany()
                   .HasForeignKey(c => c.TemplateId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.Property(cv => cv.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
            builder.ToTable("CVs");
        }
    }
}
