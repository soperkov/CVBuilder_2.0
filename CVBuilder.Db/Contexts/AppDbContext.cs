using Microsoft.EntityFrameworkCore;
using CVBuilder.Core.Models;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
