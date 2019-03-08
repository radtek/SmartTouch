CREATE TABLE [dbo].[ContactEmails] (
    [ContactEmailID] INT           IDENTITY (1, 1) NOT NULL,
    [ContactID]      INT           NOT NULL,
    [Email]          VARCHAR (256) NOT NULL,
    [EmailStatus]    SMALLINT      NOT NULL,
    [IsPrimary]      BIT           NOT NULL,
    [AccountID]      INT           NOT NULL,
    [SnoozeUntil]    DATETIME      NULL,
    [IsDeleted]      BIT           NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_missing_591]
    ON [dbo].[ContactEmails]([Email] ASC, [IsPrimary] ASC, [AccountID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_missing_66]
    ON [dbo].[ContactEmails]([ContactEmailID] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [ContactEmails_ContactID_IsPrimary_IsDeleted]
    ON [dbo].[ContactEmails]([ContactID] ASC, [IsPrimary] ASC, [IsDeleted] ASC, [EmailStatus] ASC)
    INCLUDE([Email]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [ContactEmails_ContactEmailID_ContactID_IsPrimary_IsDeleted]
    ON [dbo].[ContactEmails]([ContactEmailID] ASC, [ContactID] ASC, [IsPrimary] ASC, [IsDeleted] ASC)
    INCLUDE([Email], [EmailStatus], [AccountID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([AccountID]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-ContactEmails]
    ON [dbo].[ContactEmails]
    ON [AccountId_Scheme_Contacts] ([AccountID]);

