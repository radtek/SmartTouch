CREATE TABLE [dbo].[ContactTagMap] (
    [ContactTagMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]       INT      NOT NULL,
    [TagID]           INT      NOT NULL,
    [TaggedBy]        INT      NOT NULL,
    [TaggedOn]        DATETIME NOT NULL,
    [Accountid]       INT      NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_83]
    ON [dbo].[ContactTagMap]([ContactID] ASC, [Accountid] ASC)
    INCLUDE([TagID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([Accountid]);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_1046]
    ON [dbo].[ContactTagMap]([TagID] ASC)
    INCLUDE([ContactID], [Accountid]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([Accountid]);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_50]
    ON [dbo].[ContactTagMap]([ContactTagMapID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([Accountid]);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_229]
    ON [dbo].[ContactTagMap]([ContactID] ASC, [TagID] ASC, [Accountid] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([Accountid]);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_13]
    ON [dbo].[ContactTagMap]([TagID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([Accountid]);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_missing_48]
    ON [dbo].[ContactTagMap]([TagID] ASC)
    INCLUDE([ContactID], [Accountid]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_Contacts] ([Accountid]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-ContactTagMap]
    ON [dbo].[ContactTagMap]
    ON [AccountId_Scheme_Contacts] ([Accountid]);

