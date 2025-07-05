USE TAMHR_LOG
GO

-- Create the SyncTracking table
CREATE TABLE TB_R_SYNC_TRACKING (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(255) NOT NULL,
    SyncTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create indexes
CREATE UNIQUE INDEX UQ_SyncTracking_EntityType_EntityId 
ON TB_R_SYNC_TRACKING(EntityType, EntityId);

CREATE INDEX IX_SyncTracking_EntityType 
ON TB_R_SYNC_TRACKING(EntityType);

CREATE INDEX IX_SyncTracking_SyncTimestamp 
ON TB_R_SYNC_TRACKING(SyncTimestamp);

PRINT 'SyncTracking table created successfully';