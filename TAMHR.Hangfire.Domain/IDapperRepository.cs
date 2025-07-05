using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain
{
    public interface IDapperRepository<T>
    {
        IEnumerable<T> Find(dynamic param);
        IEnumerable<T> FindUserInternal(string username);
        IEnumerable<T> Find(string query, dynamic param);
        IEnumerable<T> FindAll();
    }
}
