using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain
{
    internal sealed class DynamicQuery
    {
        /// <summary>
        /// Gets the insert query.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The Sql query based on the item properties of the anonymous type.
        /// </returns>
        internal static string GetInsertQuery(string tableName, dynamic item, string id, string[] columns = null)
        {
            DynamicParameters parameters = new DynamicParameters();
            PropertyInfo[] props = item.GetType().GetProperties();

            foreach (var prop in props)
            {
                if (columns.Contains(prop.Name))
                {
                    parameters.Add(prop.Name, prop.GetValue(item));
                }
            }

            string[] allcolumns = props.Select(p => p.Name).ToArray();

            if (columns != null)
            {
                allcolumns = columns;
            }

            var query = string.Format("INSERT INTO {0} ({1}) OUTPUT inserted.{3} VALUES (@{2})",
                                    tableName,
                                    string.Join(",", allcolumns),
                                    string.Join(",@", allcolumns),
                                    id);
            return query;
        }

        /// <summary>
        /// Gets the update query.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The Sql query based on the item properties of the anonymous type.
        /// </returns>
        internal static string GetUpdateQuery(string tableName, dynamic item, string id, string[] columns = null)
        {
            PropertyInfo[] props = item.GetType().GetProperties();
            string[] allcolumns = props.Select(p => p.Name).ToArray();
            string[] excludeColumn = { "CreatedOn", "CreatedBy" };

            if (columns != null)
            {
                allcolumns = columns;
            }

            var newcolumns = allcolumns.Except(excludeColumn);
            var parameters = newcolumns.Select(name => name + "=@" + name).ToList();

            var query = string.Format("UPDATE {0} SET {2} WHERE {1} = @{1}", tableName, id, string.Join(",", parameters));

            return query;
        }

        /// <summary>
        /// Gets the where query.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The where clause of a query from an anonymous type.</returns>
        internal static string GetWhereQuery(dynamic item)
        {
            PropertyInfo[] props = item.GetType().GetProperties();
            string[] columns = props.Select(p => p.Name).ToArray();

            var builder = new StringBuilder();

            for (int i = 0; i < columns.Count(); i++)
            {
                string col = columns[i];
                builder.Append(col);
                builder.Append("=@");
                builder.Append(col);

                if (i < columns.Count() - 1)
                {
                    builder.Append(" AND ");
                }
            }

            return builder.ToString();
        }
    }
}
