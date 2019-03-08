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
	[CategoryID]       SMALLINT   NOT NULL DEFAULT(0),
    CONSTRAINT [PK__SentMail__96995A14ECADE945] PRIMARY KEY CLUSTERED ([SentMailDetailID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_SentMailDetails_RequestGuid]
    ON [dbo].[SentMailDetails]([RequestGuid] ASC)
    INCLUDE([Subject]);

