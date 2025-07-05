//using Hangfire;
//using Hangfire.SqlServer;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddHangfire(configuration => configuration
//       .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
//       .UseSimpleAssemblyNameTypeSerializer()
//       .UseRecommendedSerializerSettings()
//       .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
//       {
//           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
//           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
//           QueuePollInterval = TimeSpan.Zero,
//           UseRecommendedIsolationLevel = true,
//           DisableGlobalLocks = true
//       }));
//builder.Services.AddHangfireServer();

//builder.Services.AddControllers();

//builder.Services.AddRazorPages();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();

//app.UseHangfireDashboard("/dashboard");

//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapRazorPages();

//app.Run();

using Hangfire;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using TAMHR.Hangfire;
using TAMHR.Hangfire.Service;

var builder = WebApplication.CreateBuilder(args);

// Dapatkan instance startup
var startup = new Startup(builder.Configuration, builder.Environment);

// Panggil ConfigureServices sebelum Build
startup.ConfigureServices(builder.Services);

// Setelah semua service didaftarkan, build aplikasi
var app = builder.Build();

// Setelah aplikasi dibuild, service provider sudah siap
var serviceProvider = app.Services;

// Dapatkan IRecurringJobManager dari service provider
var recurringJobs = serviceProvider.GetService<IRecurringJobManager>();

// Panggil metode Configure
startup.Configure(app, builder.Environment, serviceProvider, recurringJobs);

// Jalankan aplikasi
app.Run();

//public class Program
//{
//    public static void Main(string[] args)
//    {
//        BuildWebHost(args).Run();
//    }
//    public static IWebHost BuildWebHost(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
//}