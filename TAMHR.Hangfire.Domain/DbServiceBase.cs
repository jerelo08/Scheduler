using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain
{
    public class DbServiceBase
    {
        private readonly IDbHelper _db;

        public DbServiceBase(IDbHelper db)
        {
            _db = db;
        }

        protected IDbHelper Db
        {
            get
            {
                return _db;
            }
        }

    }
}
