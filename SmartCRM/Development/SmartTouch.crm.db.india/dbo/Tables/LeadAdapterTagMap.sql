CREATE TABLE [dbo].[LeadAdapterTagMap] (
    [LeadAdapterTagMapID] INT IDENTITY (1, 1) NOT NULL,
    [LeadAdapterID]       INT NOT NULL,
    [TagID]               INT NOT NULL,
    CONSTRAINT [PK_LeadAdapterTagMap] PRIMARY KEY CLUSTERED ([LeadAdapterTagMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadAdapterTagMap_LeadAdapterAndAccountMap] FOREIGN KEY ([LeadAdapterID]) REFERENCES [dbo].[LeadAdapterAndAccountMap] ([LeadAdapterAndAccountMapID]),
    CONSTRAINT [FK_LeadAdapterTagMap_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID])
);

