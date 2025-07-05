using HangfireSchedulerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HangfireSchedulerApp.Data
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

        public DbSet<TB_R_Log> TB_R_Log { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TB_R_Log>().ToTable("TB_R_Log", schema: "dbo");
            modelBuilder.Entity<TB_R_Log>().HasKey(u => u.ID);
        }
    }
}
