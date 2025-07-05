using TAMHR.Hangfire.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TAMHR.Hangfire.Services
{
    public interface ISqlLogService
    {
        Task WriteLogAsync(
            string applicationName,
            string logCategory,
            string activity,
            string status,
            string? exceptionMessage = null,
            string? additionalInfo = null,
            string createdBy = "TAMHR.Hangfire");
    }

    public class SqlLogService : ISqlLogService
    {
        private readonly string _logConnection;

        public SqlLogService(IConfiguration configuration)
        {
            _logConnection = configuration.GetConnectionString("LogConnection") ?? throw new ArgumentNullException("LogConnection");
        }

        public async Task WriteLogAsync(
            string applicationName,
            string logCategory,
            string activity,
            string status,
            string? exceptionMessage = null,
            string? additionalInfo = null,
            string createdBy = "TAMHR.Hangfire")
        {
            using var connection = new SqlConnection(_logConnection);
            
            // Get the last log number to generate new LogID
            var lastLogNumber = await connection.QuerySingleOrDefaultAsync<int>("SELECT COUNT(*) FROM TB_R_Log");
            var newLogNumber = lastLogNumber + 1;
            var logIdFormatted = $"TAMHR_{newLogNumber.ToString("D10")}";

            var logEntry = new SchedulerLog
            {
                ID = Guid.NewGuid(),
                ApplicationName = applicationName,
                LogID = logIdFormatted,
                LogCategory = logCategory,
                Activity = activity,
                ApplicationModule = "TAMHR.Hangfire",
                IPHostName = Environment.MachineName,
                Status = status,
                AdditionalInformation = additionalInfo,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                ModifiedBy = createdBy,
                ModifiedOn = DateTime.Now,
                RowStatus = true,
                ExceptionMessage = exceptionMessage
            };

            var query = @"
                INSERT INTO TB_R_Log (ID, ApplicationName, LogID, LogCategory, Activity, ApplicationModule, 
                                    IPHostName, Status, AdditionalInformation, CreatedBy, CreatedOn, 
                                    ModifiedBy, ModifiedOn, RowStatus, ExceptionMessage)
                VALUES (@ID, @ApplicationName, @LogID, @LogCategory, @Activity, @ApplicationModule, 
                        @IPHostName, @Status, @AdditionalInformation, @CreatedBy, @CreatedOn, 
                        @ModifiedBy, @ModifiedOn, @RowStatus, @ExceptionMessage)";
            
            await connection.ExecuteAsync(query, logEntry);
        }
    }
}
