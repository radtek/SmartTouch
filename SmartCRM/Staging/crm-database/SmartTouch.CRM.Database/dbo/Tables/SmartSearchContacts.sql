CREATE TABLE [dbo].[SmartSearchContacts] (
    [ResultID]           BIGINT IDENTITY (1, 1) NOT NULL,
    [SearchDefinitionID] INT    NOT NULL,
    [AccountID]          INT    NOT NULL,
    [ContactID]          INT    NOT NULL,
    [IsActive]           BIT    NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_SmartSearchContacts_SSD_AccountId]
    ON [dbo].[SmartSearchContacts]([SearchDefinitionID] ASC, [AccountID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([AccountID]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-SmartSearchContacts]
    ON [dbo].[SmartSearchContacts]
    ON [AccountId_Scheme_Contacts] ([AccountID]);

