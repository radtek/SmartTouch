CREATE TABLE [dbo].[ContactTagMap] (
    [ContactTagMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]       INT      NOT NULL,
    [TagID]           INT      NOT NULL,
    [TaggedBy]        INT      NOT NULL,
    [TaggedOn]        DATETIME NOT NULL,
	AccountID         INT      NOT NULL,
    CONSTRAINT [PK_ContactTagMap] PRIMARY KEY CLUSTERED ([ContactTagMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ContactTagMap_Users] FOREIGN KEY ([TaggedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_ContactTags_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactTags_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]) ON DELETE CASCADE
);


GO

CREATE NONCLUSTERED INDEX [IX_ContactTagMap_ContactID_TagID]
    ON [dbo].[ContactTagMap]([ContactID] ASC, [TagID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_TagID]
    ON [dbo].[ContactTagMap]([TagID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_ContactTagMap_TagID_ContactID]
    ON [dbo].[ContactTagMap]([TagID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO

CREATE NONCLUSTERED INDEX [IX_ContactTagMap_TagID_TaggedOn]
    ON [dbo].[ContactTagMap]([TagID] ASC)
    INCLUDE([ContactTagMapID], [ContactID], [TaggedBy], [TaggedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];
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

