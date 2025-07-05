using HangfireSchedulerApp.Data;
using HangfireSchedulerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HangfireSchedulerApp.Services
{
    public class SqlLogService
    {
        private readonly LogDbContext _dbContext;

        public SqlLogService(LogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task WriteLogAsync(
            string applicationName,
            string logCategory,
            string activity,
            string status,
            string? exceptionMessage = null,
            string? additionalInfo = null,
            string createdBy = "HangfireScheduler")
        {
            var lastLogNumber = await _dbContext.TB_R_Log.CountAsync();
            var newLogNumber = lastLogNumber + 1;
            var logIdFormatted = $"TAMHR_{newLogNumber.ToString("D10")}";

            var logEntry = new TB_R_Log
            {
                ID = Guid.NewGuid(),
                ApplicationName = applicationName,
                LogID = logIdFormatted,
                LogCategory = logCategory,
                Activity = activity,
                ApplicationModule = "HangfireSchedulerApp",
                IPHostName = Environment.MachineName,
                Status = status,
                AdditionalInformation = additionalInfo,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                RowStatus = true,
                ExceptionMessage = exceptionMessage
            };

            _dbContext.TB_R_Log.Add(logEntry);
            await _dbContext.SaveChangesAsync();
        }
    }
}
