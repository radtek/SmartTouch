CREATE TABLE [dbo].[MailResponse] (
    [MailResponseID] INT              IDENTITY (1, 1) NOT NULL,
    [Token]          UNIQUEIDENTIFIER NULL,
    [RequestGuid]    UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]    DATETIME         NOT NULL,
    CONSTRAINT [PK_MailResponse] PRIMARY KEY CLUSTERED ([MailResponseID] ASC)
);

