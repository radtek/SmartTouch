CREATE TABLE [dbo].[ScheduledJobs] (
    [ScheduledJobID]    INT           IDENTITY (1, 1) NOT NULL,
    [Interval]          BIGINT        NOT NULL,
    [LastRunOn]         DATETIME      NOT NULL,
    [IsScheduledOnTime] BIT           NOT NULL,
    [NextScheduledTime] DATETIME      NOT NULL,
    [JobName]           VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_ScheduledJobs] PRIMARY KEY CLUSTERED ([ScheduledJobID] ASC)
);

