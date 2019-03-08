CREATE TABLE [dbo].[CronJobs] (
    [CronJobID]          TINYINT          NOT NULL,
    [Name]               VARCHAR (50)     NULL,
    [Description]        NVARCHAR (100)   NULL,
    [Expression]         VARCHAR (50)     NULL,
    [IsActive]           BIT              NULL,
    [LastRunOn]          DATETIME         NULL,
    [IsRunning]          BIT              NULL,
    [LastNotifyDateTime] DATETIME         NULL,
    [EstimatedTimeInMin] SMALLINT         NULL,
    [IsSqlJob]           BIT              CONSTRAINT [DF_CronJobs_IsSqlJob] DEFAULT ((0)) NOT NULL,
    [JobUniqueId]        UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([CronJobID] ASC)
);

