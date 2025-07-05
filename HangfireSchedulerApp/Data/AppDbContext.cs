using Microsoft.EntityFrameworkCore;
using HangfireSchedulerApp.Models;

namespace HangfireSchedulerApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ActualOrganizationStructure> ActualOrganizationStructures { get; set; }
        public DbSet<ActualEntityStructure> ActualEntityStructures { get; set; }
        public DbSet<OrganizationObject> OrganizationObjects { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MDM_USER
            modelBuilder.Entity<User>().ToTable("MDM_USER", schema: "dbo");
            modelBuilder.Entity<User>().HasKey(u => u.Id);

            // MDM_ACTUAL_ORGANIZATION_STRUCTURE
            modelBuilder.Entity<ActualOrganizationStructure>().ToTable("MDM_ACTUAL_ORGANIZATION_STRUCTURE", schema: "dbo");
            modelBuilder.Entity<ActualOrganizationStructure>().HasKey(a => a.Id);

            // MDM_ACTUAL_ENTITY_STRUCTURE
            modelBuilder.Entity<ActualEntityStructure>().ToTable("MDM_ACTUAL_ENTITY_STRUCTURE", schema: "dbo");
            modelBuilder.Entity<ActualEntityStructure>().HasKey(e => e.Id);

            // MDM_ORGANIZATION_OBJECT
            modelBuilder.Entity<OrganizationObject>().ToTable("MDM_ORGANIZATION_OBJECT", schema: "dbo");
            modelBuilder.Entity<OrganizationObject>().HasKey(o => o.Id);

        }
    }
}
