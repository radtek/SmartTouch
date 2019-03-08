CREATE TABLE [dbo].[ActionTagMap] (
    [ActionTagMapID] INT IDENTITY (1, 1) NOT NULL,
    [ActionID]       INT NOT NULL,
    [TagID]          INT NOT NULL,
    CONSTRAINT [PK_ActionTagMap] PRIMARY KEY CLUSTERED ([ActionTagMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ActionTags_Actions] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Actions] ([ActionID]),
    CONSTRAINT [FK_ActionTags_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]) ON DELETE CASCADE
);

