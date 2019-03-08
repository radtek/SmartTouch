CREATE TABLE [dbo].[ContactTextMessageAudit] (
    [ContactTextMessageAuditID] INT              IDENTITY (1, 1) NOT NULL,
    [ContactPhoneNumberID]      INT              NOT NULL,
    [SentBy]                    INT              NULL,
    [SentOn]                    DATETIME         NOT NULL,
    [Status]                    TINYINT          NOT NULL,
    [RequestGuid]               UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK__ContactTextMessageAudit] PRIMARY KEY CLUSTERED ([ContactTextMessageAuditID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactTextMessageAudit_ContactPhoneNumbers] FOREIGN KEY ([ContactPhoneNumberID]) REFERENCES [dbo].[ContactPhoneNumbers] ([ContactPhoneNumberID]),
    CONSTRAINT [FK_ContactTextMessageAudit_Users] FOREIGN KEY ([SentBy]) REFERENCES [dbo].[Users] ([UserID])
);

