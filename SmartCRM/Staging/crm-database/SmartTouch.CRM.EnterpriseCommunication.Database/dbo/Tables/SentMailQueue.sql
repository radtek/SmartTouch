CREATE TABLE [dbo].[SentMailQueue] (
    [SentMailQueueID]       INT              IDENTITY (1, 1) NOT NULL,
    [TokenGuid]             UNIQUEIDENTIFIER NULL,
    [RequestGuid]           UNIQUEIDENTIFIER NULL,
    [From]                  VARCHAR (500)    NULL,
    [PriorityID]            TINYINT          NULL,
    [ScheduledTime]         DATETIME         NULL,
    [QueueTime]             DATETIME         NULL,
    [StatusID]              TINYINT          NULL,
    [ServiceResponse]       VARCHAR (500)    NULL,
    [CreatedDate]           DATETIME         DEFAULT (getutcdate()) NULL,
    [GetProcessedByClassic] BIT              DEFAULT ((0)) NULL,
    CONSTRAINT [PK__SentMail__51FF6C9BD68B0C96] PRIMARY KEY CLUSTERED ([SentMailQueueID] ASC) WITH (FILLFACTOR = 90)
);

