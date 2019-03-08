CREATE TABLE [dbo].[MailSend] (
    [MailSendId]  BIGINT           IDENTITY (1, 1) NOT NULL,
    [MailGuidId]  UNIQUEIDENTIFIER NOT NULL,
    [Status]      TINYINT          NULL,
    [QueueTime]   DATETIME         NOT NULL,
    [From]        NVARCHAR (256)   NULL,
    [CreatedDate] DATETIME         NULL,
    CONSTRAINT [PK_MailSend] PRIMARY KEY CLUSTERED ([MailSendId] ASC) WITH (FILLFACTOR = 90)
);

