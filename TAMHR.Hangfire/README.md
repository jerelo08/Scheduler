# TAMHR Hangfire Data Synchronization

This project implements a robust data synchronization system using Hangfire to sync data from multiple internal databases to third-party APIs.

## Overview

The system fetches data from multiple database sources, processes it in batches, and synchronizes it with third-party APIs while preventing duplicate data transmission through a comprehensive tracking mechanism.

## Features

- **Recurring Background Job**: Configurable Hangfire job for automated data synchronization
- **Multi-Database Support**: Connects to multiple databases (Default, ESS, Log connections)
- **Batch Processing**: Configurable batch sizes for efficient API calls
- **Duplicate Prevention**: Tracks synchronized records to prevent reprocessing
- **Comprehensive Logging**: Detailed logging of all sync activities and errors
- **Error Resilience**: Continues processing even if individual batches fail
- **Unit Testing**: Complete test coverage for all sync logic

## Data Synchronization Flow

The system synchronizes data in the following order:
1. **Users** (from DefaultConnection)
2. **ActualOrg** (from DefaultConnection)  
3. **ActualEntity** (from DefaultConnection)
4. **OrgObject** (from DefaultConnection)
5. **EventsCalendar** (from EssConnection)

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "HangfireConnection": "...",
    "DefaultConnection": "...",
    "EssConnection": "...",
    "LogConnection": "..."
  },
  "CronTime": {
    "DataSyncJob": "0 3 * * *"
  },
  "SyncConfiguration": {
    "BatchSize": 3000,
    "ApiEndpoints": {
      "Users": "https://api.example.com/users",
      "ActualOrg": "https://api.example.com/actualorg",
      "ActualEntity": "https://api.example.com/actualentity",
      "OrgObject": "https://api.example.com/orgobject",
      "EventsCalendar": "https://api.example.com/eventscalendar"
    },
    "ApiTimeout": 30,
    "ApiKey": "your-api-key-here"
  }
}
```

## Database Setup

### TB_R_SYNC_TRACKING Table

Run the SQL script in `Scripts/CreateSyncTrackingTable.sql` to create the required tracking table:

```sql
CREATE TABLE TB_R_SYNC_TRACKING (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(255) NOT NULL,
    SyncTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

## Architecture

### Services

- **DataSyncService**: Main orchestrator for the synchronization process
- **SyncDataRepository**: Data access layer for fetching and tracking data
- **ApiClientService**: HTTP client wrapper for API communications
- **DataSyncJob**: Hangfire job wrapper for the sync service

### Models

- **SyncModels.cs**: Data models for all synchronized entities
- **ConfigurationModels.cs**: Configuration classes for sync settings

## Error Handling

- Failed API calls are logged but don't halt the entire process
- Failed batches are not marked as synchronized (will retry next run)
- Comprehensive error logging to SchedulerLog table
- Transient failures are handled gracefully

## Monitoring

- View job status in Hangfire Dashboard
- Check SchedulerLog table for detailed activity logs
- Monitor TB_R_SYNC_TRACKING table for synchronization progress

## Running Tests

```powershell
dotnet test TAMHR.Hangfire.Tests
```

## Deployment

1. Update connection strings in appsettings.json
2. Configure API endpoints and batch sizes
3. Run the TB_R_SYNC_TRACKING table creation script
4. Deploy the application
5. Monitor Hangfire dashboard for job execution

## Performance Considerations

- Uses pagination to avoid loading large datasets into memory
- Configurable batch sizes for optimal API performance
- Efficient SQL queries with proper indexing
- Connection pooling for database operations

## Security

- API keys configured through appsettings
- SQL injection prevention through parameterized queries
- Connection strings stored securely in configuration
