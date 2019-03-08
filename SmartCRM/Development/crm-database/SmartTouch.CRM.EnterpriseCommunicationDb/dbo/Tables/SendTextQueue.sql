CREATE TABLE [dbo].[SendTextQueue] (
    [SendTextQueueID] INT              IDENTITY (1, 1) NOT NULL,
    [RequestGuid]     UNIQUEIDENTIFIER NOT NULL,
    [TokenGuid]       UNIQUEIDENTIFIER NOT NULL,
    [ScheduledTime]   DATETIME         NULL,
    [QueueTime]       DATETIME         NULL,
    CONSTRAINT [PK_SendTextQueue] PRIMARY KEY CLUSTERED ([SendTextQueueID] ASC)
);

