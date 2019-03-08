CREATE TABLE [dbo].[ContactEmailAudit] (
    [ContactEmailAuditID] INT              IDENTITY (1, 1) NOT NULL,
    [ContactEmailID]      INT              NOT NULL,
    [SentBy]              INT              NULL,
    [SentOn]              DATETIME         NOT NULL,
    [Status]              TINYINT          NOT NULL,
    [RequestGuid]         UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK__ContactEmailAudit] PRIMARY KEY CLUSTERED ([ContactEmailAuditID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_ContactEmailAudit_Users] FOREIGN KEY ([SentBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmailAudit_ContactEmailID_Status]
    ON [dbo].[ContactEmailAudit]([ContactEmailID] ASC, [Status] ASC)
    INCLUDE([SentBy], [SentOn], [RequestGuid]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

