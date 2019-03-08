CREATE TABLE [dbo].[SentMailDetails] (
    [SentMailDetailID] INT              IDENTITY (1, 1) NOT NULL,
    [RequestGuid]      UNIQUEIDENTIFIER NOT NULL,
    [DisplayName]      VARCHAR (500)    NULL,
    [ReplyTo]          VARCHAR (500)    NULL,
    [To]               TEXT             NULL,
    [CC]               TEXT             NULL,
    [BCC]              TEXT             NULL,
    [Subject]          NVARCHAR (4000)  NULL,
    [Body]             NVARCHAR (MAX)   NULL,
    [IsBodyHtml]       BIT              NULL,
    [CreatedDate]      DATETIME         DEFAULT (getutcdate()) NULL,
    [AttachmentGUID]   UNIQUEIDENTIFIER NULL,
    [CategoryID]       SMALLINT         DEFAULT ((0)) NOT NULL,
    [AccountID]        INT              DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK__SentMail__96995A14ECADE945] PRIMARY KEY CLUSTERED ([SentMailDetailID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_SentMailDetails_RequestGuid]
    ON [dbo].[SentMailDetails]([RequestGuid] ASC)
    INCLUDE([Subject]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [SentMailDetails_RequestGuid_ReplyTo_AccountID]
    ON [dbo].[SentMailDetails]([RequestGuid] ASC, [ReplyTo] ASC, [AccountID] ASC)
    INCLUDE([SentMailDetailID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

