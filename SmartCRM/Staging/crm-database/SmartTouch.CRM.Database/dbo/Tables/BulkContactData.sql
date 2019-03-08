CREATE TABLE [dbo].[BulkContactData] (
    [BulkContactDataID] INT IDENTITY (1, 1) NOT NULL,
    [BulkOperationID]   INT NOT NULL,
    [ContactID]         INT NOT NULL,
    CONSTRAINT [PK_BulkContactData] PRIMARY KEY CLUSTERED ([BulkContactDataID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_BulkContactData_missing_437]
    ON [dbo].[BulkContactData]([BulkOperationID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

