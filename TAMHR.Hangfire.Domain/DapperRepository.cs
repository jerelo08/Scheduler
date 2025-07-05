using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain
{
    public abstract class DapperRepository<T>
    {
        protected IDbTransaction Transaction { get; set; }
        protected IDbConnection Connection { get; set; }

        public DapperRepository(IDbConnection connection, IDbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public string TableName
        {
            get
            {
                var type = typeof(T);
                var attribute = type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

                return attribute.Name;
            }
        }

        internal virtual dynamic Mapping(T item)
        {
            return item;
        }

        public virtual IEnumerable<T> Find(string query, dynamic param)
        {
            IEnumerable<T> items = null;
            //items = Connection.Query<T>("SELECT * FROM " + TableName + " WHERE " + query, (object)param, Transaction);

            var strsql = @"SELECT * FROM {0} WHERE {1}";
            strsql = string.Format(strsql, TableName, query);

            items = Connection.Query<T>(strsql, (object)param, Transaction);

            return items;
        }

        public virtual IEnumerable<T> Find(dynamic param)
        {
            return Find(DynamicQuery.GetWhereQuery(param), param);
        }

        public virtual IEnumerable<T> FindUserInternal(string username)
        {
            IEnumerable<T> items = null;

            var strsql = @"SELECT username as user_id, 
                                  display_name, 
                                  null as password, 
                                  domain as email, 
                                 'Internal' as user_type, 
                                 null as customer_group, 
                                 level_code
                         from tb_m_security_users 
                         WHERE replace(username,'TAM\','') = '{0}'";

            strsql = string.Format(strsql, username);

            items = Connection.Query<T>(strsql, Transaction);

            return items;
        }

        public virtual IEnumerable<T> FindAll()
        {
            IEnumerable<T> items = null;
            //items = Connection.Query<T>("SELECT * FROM " + TableName, Transaction);

            var strsql = @"SELECT * FROM {0}";
            strsql = string.Format(strsql, TableName);

            items = Connection.Query<T>(strsql, Transaction);

            return items;
        }
    }
}
