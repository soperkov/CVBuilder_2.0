namespace CVBuilder.Db.Entities
{
    public class UserConfiguration : IEntityTypeConfiguration<UserModel>
    {
        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.ResetToken)
                .HasMaxLength(256)
                .IsUnicode(false);

            builder.Property(u => u.ResetTokenExpires)
                .HasColumnType("datetime2");

            builder.HasMany(u => u.CVs)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.CreatedByUser)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Users");
        }
    }
}
