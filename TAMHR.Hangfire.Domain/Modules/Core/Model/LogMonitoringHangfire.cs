using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain.Modules.Core.Model
{
    public partial class LogMonitoringHangfire
    {
        public string APPLICATION_NAME { get; set; }
        public string LOG_ID { get; set; }
        public string LOG_CATEGORY { get; set; }
        public string ACTIVITY { get; set; }
        public string APPLICATION_MODULE { get; set; }
        public string USERNAME { get; set; }
        public DateTime CREATED_DATETIME { get; set; }
        public string IP_HOSTNAME { get; set; }
        public string STATUS { get; set; }
        public string ADDITIONAL_INFORMATION { get; set; }
    }
}
