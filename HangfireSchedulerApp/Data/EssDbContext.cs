using Microsoft.EntityFrameworkCore;
using HangfireSchedulerApp.Models;

namespace HangfireSchedulerApp.Data
{
    public class EssDbContext : DbContext
    {
        public DbSet<EventsCalendar> EventsCalendars { get; set; }

        public EssDbContext(DbContextOptions<EssDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventsCalendar>().ToTable("TB_M_EVENTS_CALENDAR", schema: "dbo");
            modelBuilder.Entity<EventsCalendar>().HasKey(e => e.Id);
        }
    }
}
