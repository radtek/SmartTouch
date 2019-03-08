CREATE TABLE [dbo].[SmartSearchContacts] (
    [ResultID]           BIGINT IDENTITY (1, 1) NOT NULL,
    [SearchDefinitionID] INT    NOT NULL,
    [AccountID]          INT    NOT NULL,
    [ContactID]          INT    NOT NULL,
    [IsActive]           BIT    NOT NULL
);
GO


CREATE NONCLUSTERED INDEX [IX_SmartSearchContacts_SSD_AccountId] ON [dbo].[SmartSearchContacts]
(
	[SearchDefinitionID] ASC,
	[AccountID] ASC
)
INCLUDE ( 	[ContactID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO



CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-20170202-115618] ON [dbo].[SmartSearchContacts] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0)ON [AccountId_Scheme_Contacts] ([AccountID]);

GO

