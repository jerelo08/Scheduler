using Hangfire;
using System.Net.Mail;
using System.Net;
using System.Text;
using TAMHR.Hangfire.Domain;
using TAMHR.Hangfire.Domain.Modules.Core.Model;
using TAMHR.Hangfire.Service.Config.Email;
using TAMHR.Hangfire.Service.Modules.Core.Service;

namespace TAMHR.Hangfire.Schedulers
{
    public class SchedulerJob
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SchedulerJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        [Obsolete]
        public void ScheduleJobs()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                using (var db = new DbHelper())
                {
                    ScheduleMailService scheduleMailService = new ScheduleMailService(db);

                    var schedules = scheduleMailService.GetSchedule();

                    foreach (var schedule in schedules)
                    {
                        ScheduleOrUpdateJob(schedule);
                    }
                }
            }
        }

        [Obsolete]
        private void ScheduleOrUpdateJob(ScheduleDateModel schedule)
        {
            string jobId = $"job-{schedule.Id}"; // Gunakan ID sebagai job identifier

            RecurringJob.RemoveIfExists(jobId); // Hapus jika sudah ada
            RecurringJob.AddOrUpdate(
                jobId,
                () => ExecuteJob(schedule.ScheduleName, schedule.ConfigApps),
                CronExpression(TimeSpan.Parse(schedule.ConfigTime), schedule.ConfigDays),
                TimeZoneInfo.Local
            );
        }

        public static void ExecuteJob(string jobName, string configApps)
        {
            Console.WriteLine($"Executing job: {jobName} at {DateTime.Now}");

            using (var db = new DbHelper())
            {

                MailQueueService mailQueueService = new MailQueueService(db);

                var data = mailQueueService.GetMailQueue();
                var template = mailQueueService.GetMailTemplate();

                MailMessage emailMessage = new MailMessage();

                try
                {
                    if(data != null && template != null)
                    {
                        int loop = 0;
                        var from = auth.from;

                        List<string> allowedItems = configApps.Split(';').ToList();

                        List<MailQueueModel> filteredItems = data.Where(item => allowedItems.Contains(item.Application)).ToList();

                        var orderedItems = filteredItems.OrderBy(i => i.CurrentApprover).ToList();

                        var groupedItems = orderedItems.GroupBy(i => i.CurrentApprover);

                        var firstNamesPerGroup = groupedItems.Select(g => new
                        {
                            Approver = g.Key,
                            FirstName = g.FirstOrDefault()?.Name,
                            Email = g.FirstOrDefault()?.Email,
                        }).ToList();

                        StringBuilder listTable = new StringBuilder();
                        foreach (var group in groupedItems)
                        {
                            int number = 1;
                            var body = template.MailContent;

                            body = body.Replace("{{Names}}", firstNamesPerGroup[loop].FirstName);


                            listTable = new StringBuilder();
                            listTable.Append(@"<table style=""width: 100%; border-collapse: collapse;"">
                                                <tr style=""border: 1px solid black;"">
                                                    <th style=""border: 1px solid black;"">No</th>
                                                    <th style=""border: 1px solid black;"">Application</th>
                                                    <th style=""border: 1px solid black;"">Document Type</th>
                                                    <th style=""border: 1px solid black;"">Employee Name</th>
                                                    <th style=""border: 1px solid black;"">Document Number</th>
                                                </tr>");
                            foreach (var item in group)
                            {
                                listTable.Append($@"<tr style=""border: 1px solid black;"">
                                                    <td style=""border: 1px solid black;"">{number}</td>
                                                    <td style=""border: 1px solid black;"">{item.Application}</td>
                                                    <td style=""border: 1px solid black;"">{item.FormTitle}</td>
                                                    <td style=""border: 1px solid black;"">{item.SubmitterName}</td>
                                                    <td style=""border: 1px solid black;""><a href=""{item.RedirectUrl}"">{item.DocumentNumber}</a></td>
                                                </tr>");
                                number++;
                            }
                            listTable.Append("</table>");
                            body = body.Replace("{{ListEmail}}", listTable.ToString());

                            SmtpClient emailSvc = new SmtpClient
                            {
                                Host = auth.host,
                                Port = auth.port,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(from, auth.password),
                                EnableSsl = false
                            };
                            if (from.Contains("gmail")) { emailSvc.EnableSsl = true; }

                            emailMessage = new MailMessage(from, "ibnu.kamil@ag-it.com")
                            {
                                From = new MailAddress(template.MailFrom, string.IsNullOrEmpty(template.MailFrom) ? from : template.MailFrom),
                                //emailMessage.To.RemoveAt(0);
                                Subject = string.IsNullOrEmpty(template.Subject) ? "Summary All Human Resource Application" : template.Subject,
                                Body = body,
                                IsBodyHtml = true
                            };

                            emailMessage.CC.Add("wildan.gery10@gmail.com");

                            emailSvc.Send(emailMessage);
                            loop++;
                        }
                    }                    
                }
                catch
                {

                }
            }
        }

        private string CronExpression(TimeSpan time, string days)
        {
            var daysMapping = new Dictionary<string, int>
            {
                { "Minggu", 0 },
                { "Senin", 1 },
                { "Selasa", 2 },
                { "Rabu", 3 },
                { "Kamis", 4 },
                { "Jumat", 5 },
                { "Sabtu", 6 }
            };

            var dayList = days.Split(';').Select(h => h.Trim()).ToList();
            var cronDays = dayList
                .Where(h => daysMapping.ContainsKey(h))
                .Select(h => daysMapping[h].ToString())
                .ToList();

            if (!cronDays.Any()) return null;
            string cronFormat = $"{time.Minutes} {time.Hours} * * {string.Join(",", cronDays)}";
            return cronFormat;
        }
    }
}
