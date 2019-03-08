CREATE TABLE [dbo].[ImportContactsEmailStatuses] (
    [MailGunVerificationID] INT              IDENTITY (1, 1) NOT NULL,
    [ReferenceID]           UNIQUEIDENTIFIER NULL,
    [EmailStatus]           SMALLINT         NULL
    
);


GO

CREATE NONCLUSTERED INDEX [IX_ImportContactsEmailStatuses_missing_94] ON [dbo].[ImportContactsEmailStatuses]
(
	[ReferenceID] ASC
)
INCLUDE ( 	[EmailStatus]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]

GO

CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-ImportContactsEmailStatuses] ON [dbo].[ImportContactsEmailStatuses] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0)

GO
