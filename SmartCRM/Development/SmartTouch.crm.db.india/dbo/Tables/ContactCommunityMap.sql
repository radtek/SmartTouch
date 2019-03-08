CREATE TABLE [dbo].[ContactCommunityMap] (
    [ContactCommunityMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]             INT      NOT NULL,
    [CommunityID]           SMALLINT NOT NULL,
    [CreatedOn]             DATETIME NULL,
    [CreatedBy]             INT      NULL,
    [LastModifiedOn]        DATETIME NULL,
    [LastModifiedBy]        INT      NULL,
    [IsDeleted]             BIT      NOT NULL,
    CONSTRAINT [PK_ContactCommunityMap] PRIMARY KEY CLUSTERED ([ContactCommunityMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactCommunityMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactCommunityMap_DropdownValues] FOREIGN KEY ([CommunityID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_ContactCommunityMap_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_ContactCommunityMap_Users1] FOREIGN KEY ([LastModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactCommunityMap_ContactID]
    ON [dbo].[ContactCommunityMap]([ContactID] ASC)
    INCLUDE([CommunityID]) WITH (FILLFACTOR = 90);

