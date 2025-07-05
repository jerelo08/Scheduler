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
    public partial interface IMailQueueRepository : IDapperRepository<MailQueueModel>
    {
        List<MailQueueModel> GetMailQueue();
        MailTemplateModel GetMailTemplate();
    }
    public partial class MailQueueRepository : DapperRepository<MailQueueModel>, IMailQueueRepository
    {
        public MailQueueRepository(IDbConnection connection, IDbTransaction transaction)
            : base(connection, transaction)
        {
        }

        public MailTemplateModel GetMailTemplate()
        {
            try
            {

                StringBuilder fetchquery = null;

                fetchquery = new StringBuilder(@"SELECT MailKey ,MailFrom ,Subject, MailContent
                                                FROM TB_M_EmailTemplate where MailKey = 'EmailSummaryApplication'
                                        ");
                string query = string.Format(fetchquery.ToString());
                var output = Connection.Query<MailTemplateModel>(query, commandTimeout: 3600).FirstOrDefault();
                return output;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                return null;
            }
        }

        public List<MailQueueModel> GetMailQueue()
        {
            try
            {

                var output = Connection.Query<MailQueueModel>("usp_GetEmailSummary", null,
                               null, true, commandTimeout: 0, System.Data.CommandType.StoredProcedure).ToList();

                return output;
            }
            catch (Exception ex)
            {
                string m = ex.Message;
                return null;
            }
        }
    }
}
