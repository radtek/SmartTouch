CREATE TABLE [dbo].[ReceivedMailAudit] (
    [ReceivedMailAuditID] INT           IDENTITY (1, 1) NOT NULL,
    [UserID]              INT           NOT NULL,
    [SentByContactID]     INT           NOT NULL,
    [ReceivedOn]          DATETIME      NOT NULL,
    [ReferenceID]         NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_ReceivedMailAuditID] PRIMARY KEY CLUSTERED ([ReceivedMailAuditID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ReceivedMailAudit_Contacts] FOREIGN KEY ([SentByContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ReceivedMailAudit_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ReceivedMailAudit_SentByContactID]
    ON [dbo].[ReceivedMailAudit]([SentByContactID] ASC)
    INCLUDE([UserID], [ReceivedOn], [ReferenceID]) WITH (FILLFACTOR = 90);

