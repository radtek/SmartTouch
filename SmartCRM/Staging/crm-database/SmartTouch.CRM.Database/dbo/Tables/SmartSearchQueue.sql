CREATE TABLE [dbo].[SmartSearchQueue] (
    [ResultID]           INT      IDENTITY (1, 1) NOT NULL,
    [SearchDefinitionID] INT      NOT NULL,
    [IsProcessed]        BIT      NOT NULL,
    [CreatedOn]          DATETIME NOT NULL,
    [AccountID]          INT      NOT NULL
);

