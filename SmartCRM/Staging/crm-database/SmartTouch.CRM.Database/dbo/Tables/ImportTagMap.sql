CREATE TABLE [dbo].[ImportTagMap] (
    [ImportTagMapID]      INT IDENTITY (1, 1) NOT NULL,
    [TagID]               INT NOT NULL,
    [LeadAdapterJobLogID] INT NOT NULL,
    CONSTRAINT [PK_ImportTagMap] PRIMARY KEY CLUSTERED ([ImportTagMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ImportTagMap_LeadAdapterJobLogs] FOREIGN KEY ([LeadAdapterJobLogID]) REFERENCES [dbo].[LeadAdapterJobLogs] ([LeadAdapterJobLogID]),
    CONSTRAINT [FK_ImportTagMap_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID])
);

