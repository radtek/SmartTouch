CREATE TABLE [dbo].[ContactTagMap] (
    [ContactTagMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]       INT      NOT NULL,
    [TagID]           INT      NOT NULL,
    [TaggedBy]        INT      NOT NULL,
    [TaggedOn]        DATETIME NOT NULL,
	AccountID         INT      NOT NULL
    
);


GO


CREATE NONCLUSTERED INDEX [IX_ContactTagMap_TagID_ContactID_AccountID] ON [dbo].[ContactTagMap]
(
	[TagID] ASC
)
INCLUDE ( 	[ContactID],
	[AccountID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO




Create NonClustered Index IX_ContactTagMap_missing_83 On [dbo].[ContactTagMap] ([ContactID], [AccountID]) Include ([TagID]);
GO

Create NonClustered Index IX_ContactTagMap_missing_293 On [dbo].[ContactTagMap] ([ContactID]) Include ([TagID]);
GO

Create NonClustered Index IX_ContactTagMap_missing_229 On [dbo].[ContactTagMap] ([ContactID], [TagID], [AccountID]);
GO

Create NonClustered Index IX_ContactTagMap_missing_13 On [dbo].[ContactTagMap] ([TagID]) Include ([ContactID]);
GO

Create NonClustered Index IX_ContactTagMap_missing_48 On [dbo].[ContactTagMap] ([TagID]) Include ([ContactID], [AccountID]);
GO

CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_50] ON [dbo].[ContactTagMap]
(
	[ContactTagMapID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO

CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-ContactTagMap] ON [dbo].[ContactTagMap] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [AccountId_Scheme_Contacts]([AccountID])
go






