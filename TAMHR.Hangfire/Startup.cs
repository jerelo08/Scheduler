using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TAMHR.Hangfire.Extension;
using TAMHR.Hangfire.Schedulers;
using TAMHR.Hangfire.Service;

namespace TAMHR.Hangfire
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            //services.AddHangfire(configuration => configuration
            //       .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            //       .UseSimpleAssemblyNameTypeSerializer()
            //       .UseRecommendedSerializerSettings()
            //       .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
            //       {
            //           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //           QueuePollInterval = TimeSpan.Zero,
            //           UseRecommendedIsolationLevel = true,
            //           DisableGlobalLocks = true
            //       }));
            services.ConfigureHangfire();

            services.AddHangfireServer(option =>
            {
                option.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            });
            services.AddHttpContextAccessor();

            //Register BackgroundJob
            //services.AddTransient<IHangfireJob, HangfireJob>();
            services.AddTransient<SchedulerJob>();
        }
        public void Configure(WebApplication app, IWebHostEnvironment env, IServiceProvider serviceProvider, IRecurringJobManager recurringJobs)
        {
            //Register BackgroundJob
            //serviceProvider.GetService<IHangfireJob>().RegisterSchedule();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //Hangfire
            app.UseHangfireDashboard();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            // Tambahkan job berulang setiap 10 menit
            //recurringJobs.AddOrUpdate<PvSyncService>("SyncPvToElvis", x => x.SyncPvToElvis(), "*/15 * * * *");
            var scheduler = serviceProvider.GetRequiredService<SchedulerJob>();
            scheduler.ScheduleJobs();

            RecurringJob.AddOrUpdate("update-jobs", () => scheduler.ScheduleJobs(), Cron.MinuteInterval(10), TimeZoneInfo.Local);

            app.Run();
        }
        

    }
}
