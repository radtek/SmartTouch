CREATE TABLE [dbo].[IndexData] (
    [ReferenceID] UNIQUEIDENTIFIER NOT NULL,
    [EntityID]    INT              NOT NULL,
    [IndexType]   INT              NOT NULL,
    [CreatedOn]   DATETIME         NOT NULL,
    [IndexedOn]   DATETIME         NULL,
    [Status]      INT              NOT NULL,
	[IsPercolationNeeded]   BIT  NOT NULL DEFAULT(1),
    CONSTRAINT [PK_ModuleIndexing] PRIMARY KEY CLUSTERED ([ReferenceID] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_IndexData_EntityID]
    ON [dbo].[IndexData]([EntityID] ASC)
    INCLUDE([ReferenceID], [IndexType], [CreatedOn], [IndexedOn], [Status]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_IndexData_Status]
    ON [dbo].[IndexData]([Status] ASC)
    INCLUDE([ReferenceID], [EntityID], [IndexType], [CreatedOn], [IndexedOn]) WITH (FILLFACTOR = 90);

