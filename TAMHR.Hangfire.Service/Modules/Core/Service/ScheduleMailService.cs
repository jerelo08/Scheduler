using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.Hangfire.Domain;
using TAMHR.Hangfire.Domain.Modules.Core.Model;

namespace TAMHR.Hangfire.Service.Modules.Core.Service
{
    public class ScheduleMailService : DbServiceBase
    {
        public ScheduleMailService(IDbHelper db) : base(db)
        {
        }

        public List<ScheduleDateModel> GetSchedule()
        {
            var result = Db.ScheduleDateRepository.GetSchedule();
            return result;
        }
    }
}
