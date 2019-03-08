CREATE TABLE [dbo].[CronJobHistory_Archives] (
    [CronJobHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [CronJobID]        TINYINT        NULL,
    [StartTime]        DATETIME       NULL,
    [EndTime]          DATETIME       NULL,
    [Remarks]          VARCHAR (1000) NULL
);

