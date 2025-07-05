using Hangfire;
using TAMHR.Hangfire.Helper;
using Newtonsoft.Json;
using TAMHR.Hangfire.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TAMHR.Hangfire.Domain;
using TAMHR.Hangfire.Service.Modules.Core.Service;
using System.Net;
using Dapper;
using TAMHR.Hangfire.Domain.Modules.Core.Model;
using System.Net.Mail;
using TAMHR.Hangfire.Service.Config.Email;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace TAMHR.Hangfire.Schedulers
{
    public interface IHangfireJob
    {
        void RegisterSchedule();
    }
    public class HangfireJob : IHangfireJob
    {
        public void RegisterSchedule()
        {
            RunService();
        }

        public void RunService()
        {
            //Register BackgroundJob
            //RecurringJob.AddOrUpdate(() => TestGetView(), AppConstants.GetAppSetting("CronTime:CreateDaFixAmount"), TimeZoneInfo.Local);
        }
    }
}