using Hangfire;
using Hangfire.Server;
using HangfireSchedulerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            var schedulerService = scope.ServiceProvider.GetRequiredService<SchedulerService>();

            // Daftarkan recurring job dengan jadwal setiap hari jam 03:00 WIB
            //recurringJobManager.AddOrUpdate(
            //    "daily-job-03-00",
            //    () => schedulerService.RunJobWrapperAsync(),
            //    "0 3 * * *",  // Cron: Jam 03:00 pagi setiap hari
            //    new RecurringJobOptions
            //    {
            //        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            //    }
            //);
            recurringJobManager.AddOrUpdate(
                "daily-job-11-15",
                () => schedulerService.RunJobWrapperAsync(),
                "15 11 * * *",  // Cron: Jam 03:00 pagi setiap hari
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );


            Console.WriteLine($"[Hangfire] Job 'daily-job-03-00' scheduled at {DateTime.Now:HH:mm:ss}");

            // Jalankan 1x saat service pertama kali start (Opsional)
            await schedulerService.RunJobAsync();

            // Biarkan Hangfire server terus berjalan
            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("[Hangfire] BackgroundJobServer running...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }
}
