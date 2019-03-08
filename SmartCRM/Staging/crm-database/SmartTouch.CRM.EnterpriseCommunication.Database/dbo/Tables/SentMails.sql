CREATE TABLE [dbo].[SentMails] (
    [SentMailID]      INT              IDENTITY (1, 1) NOT NULL,
    [TokenGuid]       UNIQUEIDENTIFIER NULL,
    [RequestGuid]     UNIQUEIDENTIFIER NULL,
    [From]            VARCHAR (500)    NULL,
    [PriorityID]      TINYINT          NULL,
    [ScheduledTime]   DATETIME         NULL,
    [QueueTime]       DATETIME         NULL,
    [StatusID]        TINYINT          NULL,
    [ServiceResponse] VARCHAR (500)    NULL,
    CONSTRAINT [PK__SentMail__0F48BE5C246606D8] PRIMARY KEY CLUSTERED ([SentMailID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_SentMails_TokenGuid]
    ON [dbo].[SentMails]([TokenGuid] ASC)
    INCLUDE([RequestGuid]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_SentMails_ScheduledTime]
    ON [dbo].[SentMails]([ScheduledTime] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_SentMails_RequestGuid]
    ON [dbo].[SentMails]([RequestGuid] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

