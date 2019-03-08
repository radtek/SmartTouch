CREATE TABLE [dbo].[Operations] (
    [OperationID]   SMALLINT      IDENTITY (1, 1) NOT NULL,
    [OperationName] NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_Operations] PRIMARY KEY CLUSTERED ([OperationID] ASC) WITH (FILLFACTOR = 90)
);

