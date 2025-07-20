namespace CVBuilder.Db.Entities
{
    public class SkillConfiguration : IEntityTypeConfiguration<SkillModel>
    {
        public void Configure(EntityTypeBuilder<SkillModel> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne<CVModel>()
               .WithMany(s => s.Skills)
               .HasForeignKey(s => s.CVId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Skills");
        }
    }
}
