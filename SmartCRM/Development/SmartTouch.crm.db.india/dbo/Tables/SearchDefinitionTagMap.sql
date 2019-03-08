CREATE TABLE [dbo].[SearchDefinitionTagMap] (
    [SearchDefinitionTagMapID] INT IDENTITY (1, 1) NOT NULL,
    [SearchDefinitionID]       INT NOT NULL,
    [SearchDefinitionTagID]    INT NOT NULL,
    CONSTRAINT [PK_SearchDefinitionTagMap] PRIMARY KEY CLUSTERED ([SearchDefinitionTagMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SearchDefinitionTagMap_SearchDefinitions] FOREIGN KEY ([SearchDefinitionID]) REFERENCES [dbo].[SearchDefinitions] ([SearchDefinitionID]),
    CONSTRAINT [FK_SearchDefinitionTagMap_Tags1] FOREIGN KEY ([SearchDefinitionTagID]) REFERENCES [dbo].[Tags] ([TagID])
);

