-- Create SyncTracking table in the LogConnection database
-- This table tracks all records that have been successfully synchronized to prevent duplicates

USE TAMHR_LOG
GO

-- Drop table if exists (for development purposes)
IF OBJECT_ID('dbo.SyncTracking', 'U') IS NOT NULL
    DROP TABLE dbo.SyncTracking;
GO

-- Create the SyncTracking table
CREATE TABLE SyncTracking (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL,    -- e.g., 'User', 'ActualOrg', 'ActualEntity', 'OrgObject', 'EventsCalendar'
    EntityId NVARCHAR(255) NOT NULL,      -- The primary key of the source record (string to be flexible)
    SyncTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create a unique index to prevent duplicate entries and speed up lookups
CREATE UNIQUE INDEX UQ_SyncTracking_EntityType_EntityId 
ON SyncTracking(EntityType, EntityId);

-- Create an index on EntityType for faster queries
CREATE INDEX IX_SyncTracking_EntityType 
ON SyncTracking(EntityType);

-- Create an index on SyncTimestamp for reporting purposes
CREATE INDEX IX_SyncTracking_SyncTimestamp 
ON SyncTracking(SyncTimestamp);

PRINT 'SyncTracking table created successfully';
