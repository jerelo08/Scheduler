using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TAMHR.Hangfire.Service.Config.Email;

namespace TAMHR.Hangfire.Service.Modules.Core.Service
{
    public class SendMail
    {
        public static void f(
                    string businessUnit,
                    string type,
                    string subject,
                    string body,
                    string subPath
                    )
        {
            MailMessage emailMessage = new MailMessage();
            try
            {
                var from = auth.from;
                SmtpClient emailSvc = new SmtpClient
                {
                    Host = auth.host,
                    Port = auth.port,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from, auth.password),
                    EnableSsl = false
                };
                if (from.Contains("gmail")) { emailSvc.EnableSsl = true; }

                emailMessage = new MailMessage(from, "ate")
                {
                    From = new MailAddress(from),
                    //emailMessage.To.RemoveAt(0);
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                var Now = DateTime.Now.Date;

                //emailMessage.CC.Clear();

                var cap = 9 * 1000000;

                emailSvc.Send(emailMessage);
                var end = "";
            }
            catch (Exception e)
            {
               
            }
        }
    }
}
