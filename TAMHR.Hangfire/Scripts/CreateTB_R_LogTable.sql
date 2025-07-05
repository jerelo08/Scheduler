-- Create TB_R_Log table in the LogConnection database
-- This table is used for storing detailed logs from the TAMHR Hangfire scheduler

USE TAMHR_LOG
GO

-- Drop table if exists (for development purposes)
IF OBJECT_ID('dbo.TB_R_Log', 'U') IS NOT NULL
    DROP TABLE dbo.TB_R_Log;
GO

-- Create the TB_R_Log table (matching the SchedulerLog model)
CREATE TABLE TB_R_Log (
    ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ApplicationName NVARCHAR(255) NOT NULL,
    LogID NVARCHAR(255) NOT NULL,
    LogCategory NVARCHAR(100) NOT NULL,
    Activity NVARCHAR(500) NOT NULL,
    ApplicationModule NVARCHAR(255) NOT NULL,
    IPHostName NVARCHAR(255) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    AdditionalInformation NVARCHAR(MAX) NULL,
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(255) NOT NULL,
    ModifiedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RowStatus BIT NOT NULL DEFAULT 1,
    ExceptionMessage NVARCHAR(MAX) NULL
);

-- Create indexes for better performance
CREATE INDEX IX_TB_R_Log_ApplicationName ON TB_R_Log(ApplicationName);
CREATE INDEX IX_TB_R_Log_LogCategory ON TB_R_Log(LogCategory);
CREATE INDEX IX_TB_R_Log_CreatedOn ON TB_R_Log(CreatedOn);
CREATE INDEX IX_TB_R_Log_Status ON TB_R_Log(Status);

PRINT 'TB_R_Log table created successfully';
