-- Create TB_R_SYNC_TRACKING table in the LogConnection database
-- This table tracks all records that have been successfully synchronized to prevent duplicates

USE TAMHR_LOG
GO

-- Drop table if exists (for development purposes)
IF OBJECT_ID('dbo.TB_R_SYNC_TRACKING', 'U') IS NOT NULL
    DROP TABLE dbo.TB_R_SYNC_TRACKING;
GO

-- Create the TB_R_SYNC_TRACKING table
CREATE TABLE TB_R_SYNC_TRACKING (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,    -- e.g., 'User', 'ActualOrg', 'ActualEntity', 'OrgObject', 'EventsCalendar'
    EntityId NVARCHAR(255) NOT NULL,      -- The primary key of the source record (string to be flexible)
    SyncTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create a unique index to prevent duplicate entries and speed up lookups
CREATE UNIQUE INDEX UQ_TB_R_SYNC_TRACKING_EntityType_EntityId 
ON TB_R_SYNC_TRACKING(EntityType, EntityId);

-- Create an index on EntityType for faster queries
CREATE INDEX IX_TB_R_SYNC_TRACKING_EntityType 
ON TB_R_SYNC_TRACKING(EntityType);

-- Create an index on SyncTimestamp for reporting purposes
CREATE INDEX IX_TB_R_SYNC_TRACKING_SyncTimestamp 
ON TB_R_SYNC_TRACKING(SyncTimestamp);

PRINT 'TB_R_SYNC_TRACKING table created successfully';
