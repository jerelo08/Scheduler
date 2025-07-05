using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.Hangfire.Domain;

namespace TAMHR.Hangfire.Service.Modules.Core.Service
{
    public class LogMonitoringHangfireService : DbServiceBase
    {
        public LogMonitoringHangfireService(IDbHelper db) : base(db)
        {
        }

        public bool LogMonitoringProcess(string application_module, string status, string add_info, string activity, string myIP)
        {
            Db.Connection.Open();
            var transact = Db.Connection.BeginTransaction();
            try
            {
                var result = Db.LogMonitoringHangfireRepository.LogMonitoringProcess(transact, application_module, status, add_info, activity, myIP);
                transact.Commit();
            }
            catch (Exception ex)
            {
                return false;
            }
            Db.Connection.Close();
            return true;
        }
    }
}
