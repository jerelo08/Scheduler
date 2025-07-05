using System.Data.Common;
using System.Data;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace TAMHR.Hangfire.Domain
{
    public abstract class DbContext
    {
        protected IDbConnection _connection;
        protected IDbTransaction _transaction;

        //protected DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");

        public DbContext()
        {
            _connection = OpenConnection();
            //_connection = factory.CreateConnection();
            //_connection.ConnectionString = ConfigurationManager.ConnectionStrings["constring"].ConnectionString;
        }

        private IDbConnection OpenConnection()
        {
            var config = new AppConfiguration();
            var builder = new SqlConnectionStringBuilder(config.ConnectionString);
            builder.ConnectTimeout = 0;
            var conn = new SqlConnection(builder.ConnectionString);

            return conn;
        }

        public DbContext(bool UseTransaction)
        {
            _connection = OpenConnection();
            if (UseTransaction)
            {
                _connection.Open();
                _transaction = _connection.BeginTransaction();
            }
        }

        //public DbContext(bool UseTransaction = false)
        //{
        //    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        //    var connectionString = config["ConnectionStrings:HangfireConnection"];
        //    //_connection = factory.CreateConnection();
        //    //_connection.ConnectionString = ConfigurationManager.ConnectionStrings["constring"].ConnectionString;
        //    //if (UseTransaction)
        //    //{
        //    //    _connection.Open();
        //    //    _transaction = _connection.BeginTransaction();
        //    //}
        //}

        protected bool _disposed;

        public void Commit()
        {

            if (_transaction == null)
                return;
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                resetRepositories();
            }
        }

        public virtual void resetRepositories()
        {
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }

        ~DbContext()
        {
            dispose(false);
        }
    }
}