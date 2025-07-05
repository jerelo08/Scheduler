using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.Hangfire.Domain;
using TAMHR.Hangfire.Domain.Modules.Core.Model;

namespace TAMHR.Hangfire.Service.Modules.Core.Service
{
    public class MailQueueService : DbServiceBase
    {
        public MailQueueService(IDbHelper db) : base(db)
        {
        }

        public List<MailQueueModel> GetMailQueue()
        {
            var result = Db.MailQueueRepository.GetMailQueue();
            return result;
        }

        public MailTemplateModel GetMailTemplate()
        {
            var result = Db.MailQueueRepository.GetMailTemplate();
            return result;
        }
    }
}
