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
    public partial interface IScheduleDateRepository : IDapperRepository<ScheduleDateModel>
    {
        List<ScheduleDateModel> GetSchedule();
    }
    public partial class ScheduleDateRepository : DapperRepository<ScheduleDateModel>, IScheduleDateRepository
    {
        public ScheduleDateRepository(IDbConnection connection, IDbTransaction transaction)
            : base(connection, transaction)
        {
        }

        public List<ScheduleDateModel> GetSchedule()
        {
            try
            {

                StringBuilder fetchquery = null;

                fetchquery = new StringBuilder(@"SELECT *
                                    FROM [TB_M_MAIL_SCHEDULER] WITH (NOLOCK)
                                        ");
                Console.WriteLine("Start fetching data...");
                string query = string.Format(fetchquery.ToString());
                var output = Connection.Query<ScheduleDateModel>(query, commandTimeout: 3600).ToList();
                Console.WriteLine($"Data fetched: {output.Count} rows.");
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
