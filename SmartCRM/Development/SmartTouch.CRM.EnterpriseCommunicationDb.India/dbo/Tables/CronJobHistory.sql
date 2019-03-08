﻿CREATE TABLE [dbo].[CronJobHistory] (
    [CronJobHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [CronJobID]        TINYINT        NULL,
    [StartTime]        DATETIME       NULL,
    [EndTime]          DATETIME       NULL,
    [Remarks]          VARCHAR (1000) NULL,
    PRIMARY KEY CLUSTERED ([CronJobHistoryID] ASC),
    CONSTRAINT [CronJobHistory_CronJobs] FOREIGN KEY ([CronJobID]) REFERENCES [dbo].[CronJobs] ([CronJobID])
);
