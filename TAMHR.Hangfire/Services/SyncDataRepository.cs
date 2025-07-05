using Dapper;
using Microsoft.Data.SqlClient;
using TAMHR.Hangfire.Models;
using System.Data;

namespace TAMHR.Hangfire.Services
{
    public interface ISyncDataRepository
    {
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> excludeIds, int skip, int take);
        Task<IEnumerable<ActualOrganizationStructure>> GetActualOrgAsync(IEnumerable<string> excludeIds, int skip, int take);
        Task<IEnumerable<ActualEntityStructure>> GetActualEntityAsync(IEnumerable<string> excludeIds, int skip, int take);
        Task<IEnumerable<OrganizationObject>> GetOrgObjectAsync(IEnumerable<string> excludeIds, int skip, int take);
        Task<IEnumerable<EventsCalendar>> GetEventsCalendarAsync(IEnumerable<string> excludeIds, int skip, int take);
        Task<IEnumerable<string>> GetSyncedEntityIdsAsync(string entityType);
        Task<bool> AddSyncTrackingAsync(string entityType, IEnumerable<string> entityIds);
        Task<bool> LogActivityAsync(SchedulerLog log);
    }

    public class SyncDataRepository : ISyncDataRepository
    {
        private readonly string _defaultConnection;
        private readonly string _essConnection;
        private readonly string _logConnection;

        public SyncDataRepository(IConfiguration configuration)
        {
            _defaultConnection = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
            _essConnection = configuration.GetConnectionString("EssConnection") ?? throw new ArgumentNullException("EssConnection");
            _logConnection = configuration.GetConnectionString("LogConnection") ?? throw new ArgumentNullException("LogConnection");
        }

        public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> excludeIds, int skip, int take)
        {
            using var connection = new SqlConnection(_defaultConnection);
            var excludeClause = excludeIds.Any() ? $"AND Id NOT IN ({string.Join(",", excludeIds.Select(id => $"'{id}'"))})" : "";
            
            var query = $@"
                SELECT Id, NoReg, Username, Name, Gender, Email, IsActive
                FROM Users
                WHERE 1=1 {excludeClause}
                ORDER BY Id
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            
            return await connection.QueryAsync<User>(query, new { Skip = skip, Take = take });
        }

        public async Task<IEnumerable<ActualOrganizationStructure>> GetActualOrgAsync(IEnumerable<string> excludeIds, int skip, int take)
        {
            using var connection = new SqlConnection(_defaultConnection);
            var excludeClause = excludeIds.Any() ? $"AND Id NOT IN ({string.Join(",", excludeIds.Select(id => $"'{id}'"))})" : "";
            
            var query = $@"
                SELECT Id, OrgCode, ParentOrgCode, OrgName, Service, NoReg, Name, PostCode, PostName, 
                       JobCode, JobName, EmployeeGroup, EmployeeGroupText, EmployeeSubgroup, EmployeeSubgroupText,
                       WorkContract, WorkContractText, PersonalArea, PersonalSubarea, DepthLevel, Staffing,
                       Chief, OrgLevel, Vacant, Structure, ObjectDescription
                FROM ActualOrganizationStructure
                WHERE 1=1 {excludeClause}
                ORDER BY Id
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            
            return await connection.QueryAsync<ActualOrganizationStructure>(query, new { Skip = skip, Take = take });
        }

        public async Task<IEnumerable<ActualEntityStructure>> GetActualEntityAsync(IEnumerable<string> excludeIds, int skip, int take)
        {
            using var connection = new SqlConnection(_defaultConnection);
            var excludeClause = excludeIds.Any() ? $"AND Id NOT IN ({string.Join(",", excludeIds.Select(id => $"'{id}'"))})" : "";
            
            var query = $@"
                SELECT Id, OrgCode, ObjectCode, ObjectText, ObjectDescription
                FROM ActualEntityStructure
                WHERE 1=1 {excludeClause}
                ORDER BY Id
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            
            return await connection.QueryAsync<ActualEntityStructure>(query, new { Skip = skip, Take = take });
        }

        public async Task<IEnumerable<OrganizationObject>> GetOrgObjectAsync(IEnumerable<string> excludeIds, int skip, int take)
        {
            using var connection = new SqlConnection(_defaultConnection);
            var excludeClause = excludeIds.Any() ? $"AND Id NOT IN ({string.Join(",", excludeIds.Select(id => $"'{id}'"))})" : "";
            
            var query = $@"
                SELECT Id, ObjectID, ObjectType, Abbreviation, ObjectText, StartDate, EndDate, ObjectDescription
                FROM OrganizationObject
                WHERE 1=1 {excludeClause}
                ORDER BY Id
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            
            return await connection.QueryAsync<OrganizationObject>(query, new { Skip = skip, Take = take });
        }

        public async Task<IEnumerable<EventsCalendar>> GetEventsCalendarAsync(IEnumerable<string> excludeIds, int skip, int take)
        {
            using var connection = new SqlConnection(_essConnection);
            var excludeClause = excludeIds.Any() ? $"AND Id NOT IN ({string.Join(",", excludeIds.Select(id => $"'{id}'"))})" : "";
            
            var query = $@"
                SELECT Id, Title, EventTypeCode, StartDate, EndDate, Description, CreatedBy, CreatedOn,
                       ModifiedBy, ModifiedOn, RowStatus
                FROM EventsCalendar
                WHERE 1=1 {excludeClause}
                ORDER BY Id
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            
            return await connection.QueryAsync<EventsCalendar>(query, new { Skip = skip, Take = take });
        }

        public async Task<IEnumerable<string>> GetSyncedEntityIdsAsync(string entityType)
        {
            using var connection = new SqlConnection(_logConnection);
            var query = "SELECT EntityId FROM TB_R_SYNC_TRACKING WHERE EntityType = @EntityType";
            return await connection.QueryAsync<string>(query, new { EntityType = entityType });
        }

        public async Task<bool> AddSyncTrackingAsync(string entityType, IEnumerable<string> entityIds)
        {
            using var connection = new SqlConnection(_logConnection);
            
            var values = entityIds.Select(id => $"('{entityType}', '{id}', GETUTCDATE())");
            var query = $@"
                INSERT INTO TB_R_SYNC_TRACKING (EntityType, EntityId, SyncTimestamp)
                VALUES {string.Join(", ", values)}";
            
            var result = await connection.ExecuteAsync(query);
            return result > 0;
        }

        public async Task<bool> LogActivityAsync(SchedulerLog log)
        {
            using var connection = new SqlConnection(_logConnection);
              var query = @"
                INSERT INTO TB_R_Log (ID, ApplicationName, LogID, LogCategory, Activity, ApplicationModule, 
                                    IPHostName, Status, AdditionalInformation, CreatedBy, CreatedOn, 
                                    ModifiedBy, ModifiedOn, RowStatus, ExceptionMessage)
                VALUES (@ID, @ApplicationName, @LogID, @LogCategory, @Activity, @ApplicationModule, 
                        @IPHostName, @Status, @AdditionalInformation, @CreatedBy, @CreatedOn, 
                        @ModifiedBy, @ModifiedOn, @RowStatus, @ExceptionMessage)";
            
            var result = await connection.ExecuteAsync(query, log);
            return result > 0;
        }
    }
}
