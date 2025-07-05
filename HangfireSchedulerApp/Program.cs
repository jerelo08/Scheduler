using System.Net;
using Hangfire;
using Hangfire.MemoryStorage;
using HangfireSchedulerApp.Data;
using HangfireSchedulerApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

// Paksa HttpClient menggunakan TLS 1.2 dan 1.3
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()  // Agar bisa jalan sebagai Windows Service
    .ConfigureServices((hostContext, services) =>
    {
        // Load konfigurasi dari appsettings.json
        services.AddSingleton<IConfiguration>(configuration);

        // Register DB Context untuk masing-masing database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<EssDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("EssConnection")));

        services.AddDbContext<LogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("LogConnection")));

        // Register HttpClient untuk API call
        services.AddHttpClient<SchedulerService>();

        // Setup Hangfire dengan Memory Storage
        services.AddHangfire(config => config.UseMemoryStorage());
        services.AddHangfireServer();
        services.AddSingleton<IRecurringJobManager, RecurringJobManager>();

        // Register custom logging service
        services.AddScoped<SqlLogService>();

        // Register worker utama
        services.AddHostedService<Worker>();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureKestrel(app =>
        {
            var port = configuration.GetValue<int>("KestrelSettings:Port");
            app.ListenAnyIP(port);
        });
        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard("/hangfire");
            });
        });
    })
    .Build();


await host.RunAsync();
