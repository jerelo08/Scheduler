using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.Hangfire.Domain.Modules.Core.Repository;

namespace TAMHR.Hangfire.Domain
{
    public partial interface IDbHelper : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        ILogMonitoringHangfireRepository LogMonitoringHangfireRepository { get; }
        IMailQueueRepository MailQueueRepository { get; }
        IScheduleDateRepository ScheduleDateRepository { get; }
    }
    public class DbHelper : DbContext, IDbHelper
    {
        public DbHelper(bool UseTransaction = false) : base(UseTransaction)
        {
        }

        private ILogMonitoringHangfireRepository _LogMonitoringHangfireRepository;
        private IMailQueueRepository _MailQueueRepository;
        private IScheduleDateRepository _ScheduleDateRepository;

        public IDbConnection Connection
        {
            get
            {
                return base._connection;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return base._transaction;
            }
        }
        public ILogMonitoringHangfireRepository LogMonitoringHangfireRepository
        {
            get { return _LogMonitoringHangfireRepository ?? (_LogMonitoringHangfireRepository = new LogMonitoringHangfireRepository(_connection, _transaction)); }
        }

        public IMailQueueRepository MailQueueRepository
        {
            get { return _MailQueueRepository ?? (_MailQueueRepository = new MailQueueRepository(_connection, _transaction)); }
        }

        public IScheduleDateRepository ScheduleDateRepository
        {
            get { return _ScheduleDateRepository ?? (_ScheduleDateRepository = new ScheduleDateRepository(_connection, _transaction)); }
        }
    }
}
