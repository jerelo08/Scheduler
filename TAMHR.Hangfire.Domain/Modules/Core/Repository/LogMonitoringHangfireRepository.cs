using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.Hangfire.Domain.Modules.Core.Model;

namespace TAMHR.Hangfire.Domain.Modules.Core.Repository
{
    public partial interface ILogMonitoringHangfireRepository : IDapperRepository<LogMonitoringHangfire>
    {
        bool LogMonitoringProcess(IDbTransaction transaction, string application_module, string status, string add_info, string activity, string myIp);
    }

    public partial class LogMonitoringHangfireRepository : DapperRepository<LogMonitoringHangfire>, ILogMonitoringHangfireRepository
    {
        public LogMonitoringHangfireRepository(IDbConnection connection, IDbTransaction transaction)
            : base(connection, transaction)
        {
        }

        public bool LogMonitoringProcess(IDbTransaction transaction, string application_module, string status, string add_info, string activity, string myIp)
        {
            string err_mesg = "";
            //string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (string.IsNullOrEmpty(ip))
            //{
            //    ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            //}
            var parameters = new Dictionary<string, object>();
            parameters.Add("@AppName", application_module);
            parameters.Add("@LogCategory", "Data Changes");
            parameters.Add("Activity", activity);
            parameters.Add("AppModule", "SWAPPING");
            parameters.Add("Username", application_module);
            parameters.Add("Hostname", myIp);
            parameters.Add("Status", status);
            parameters.Add("AdditionalInformation", add_info);

            var output = Connection.Execute("USP_CREATE_LOG", parameters, transaction, commandTimeout: 0, commandType: System.Data.CommandType.StoredProcedure);

            return true;
        }
    }
}
