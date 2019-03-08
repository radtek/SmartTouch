CREATE TABLE [dbo].[BulkContactData] (
    [BulkContactDataID] INT IDENTITY (1, 1) NOT NULL,
    [BulkOperationID]   INT NOT NULL,
    [ContactID]         INT NOT NULL,
    CONSTRAINT [PK_BulkContactData] PRIMARY KEY CLUSTERED ([BulkContactDataID] ASC) WITH (FILLFACTOR = 90)
);

