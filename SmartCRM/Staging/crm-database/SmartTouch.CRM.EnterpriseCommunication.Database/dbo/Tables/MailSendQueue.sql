CREATE TABLE [dbo].[MailSendQueue] (
    [MailSendQueueId] BIGINT           IDENTITY (1, 1) NOT NULL,
    [MailguidId]      UNIQUEIDENTIFIER NOT NULL,
    [QueueTime]       DATETIME         NOT NULL,
    [From]            NVARCHAR (256)   NOT NULL,
    [Status]          TINYINT          NOT NULL,
    [CreateDate]      DATETIME         NOT NULL,
    CONSTRAINT [PK_MailSendQueue] PRIMARY KEY CLUSTERED ([MailSendQueueId] ASC) WITH (FILLFACTOR = 90)
);

