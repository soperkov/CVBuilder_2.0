using Microsoft.EntityFrameworkCore.Internal;

namespace CVBuilder.Db.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<CVModel> CVs { get; set; }
        public DbSet<TemplateModel> Templates { get; set; }
        public DbSet<SkillModel> Skills { get; set; }
        public DbSet<EducationEntryModel> Educations { get; set; }
        public DbSet<EmploymentEntryModel> Employments { get; set; }
        public DbSet<LanguageModel> Languages { get; set; }
        public DbSet<LanguageEntryModel> LanguageEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
