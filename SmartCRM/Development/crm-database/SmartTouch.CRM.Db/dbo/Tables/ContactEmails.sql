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
CREATE NONCLUSTERED INDEX [IX_ContactEmails_AccountID]
    ON [dbo].[ContactEmails]([AccountID] ASC)
    INCLUDE([ContactEmailID], [ContactID], [Email], [EmailStatus], [IsPrimary]) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_ContactEmails_AccountID_IsDeleted]
    ON [dbo].[ContactEmails]([AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([ContactEmailID], [ContactID], [Email], [EmailStatus], [IsPrimary], [SnoozeUntil]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_ContactID_ContactID]
    ON [dbo].[ContactEmails]([ContactID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_Email]
    ON [dbo].[ContactEmails]([Email] ASC)
    INCLUDE([ContactEmailID], [ContactID], [EmailStatus], [IsPrimary], [AccountID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_IsDeleted]
    ON [dbo].[ContactEmails]([IsDeleted] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_ContactEmails_IsPrimary_AccountID_EmailStatus_IsDeleted]
    ON [dbo].[ContactEmails]([IsPrimary] ASC, [AccountID] ASC, [EmailStatus] ASC, [IsDeleted] ASC)
    INCLUDE([ContactEmailID], [ContactID], [Email], [SnoozeUntil]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_IsPrimary_AccountID_IsDeleted]
    ON [dbo].[ContactEmails]([IsPrimary] ASC, [AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID], [Email]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_IsPrimary_IsDeleted]
    ON [dbo].[ContactEmails]([IsPrimary] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID], [EmailStatus]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactEmails_missing_160]
    ON [dbo].[ContactEmails]([EmailStatus] ASC)
    INCLUDE([ContactEmailID], [ContactID], [Email], [IsPrimary], [AccountID], [SnoozeUntil]) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_ContactEmails_IsDeleted_ContactID_Email_AccountID] ON [dbo].[ContactEmails]
(
	[IsDeleted] ASC
)
INCLUDE ( 	[ContactID],
	[Email],
	[AccountID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO


Create NonClustered Index IX_ContactEmails_missing_16184 On [dbo].[ContactEmails] ([IsPrimary], [AccountID], [IsDeleted],[EmailStatus]) Include ([ContactID]);
GO


CREATE NONCLUSTERED INDEX [IX_ContactEmails_missing_66] ON [dbo].[ContactEmails]
(
	[ContactEmailID] ASC,
	[IsDeleted] ASC
)
INCLUDE ( 	[ContactID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO


CREATE NONCLUSTERED INDEX [IX_ContactEmails_missing_61] ON [dbo].[ContactEmails]
(
	[IsPrimary] ASC,
	[IsDeleted] ASC,
	[EmailStatus] ASC
)
INCLUDE ( 	[ContactID],
	[AccountID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO



CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-ContactEmails] ON [dbo].[ContactEmails] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) 
GO
