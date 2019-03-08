CREATE TABLE [dbo].[ImportCustomData] (
    [ImportCustomDataID] INT              IDENTITY (1, 1) NOT NULL,
    [FieldID]            INT              NULL,
    [FieldTypeID]        INT              NULL,
    [FieldValue]         NVARCHAR (MAX)   NULL,
    [ReferenceID]        UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_ImportCustomData] PRIMARY KEY CLUSTERED ([ImportCustomDataID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_ImportCustomData]
    ON [dbo].[ImportCustomData]([ReferenceID] ASC)
    INCLUDE([ImportCustomDataID], [FieldID], [FieldTypeID], [FieldValue]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ImportCustomData_FieldTypeID]
    ON [dbo].[ImportCustomData]([FieldTypeID] ASC)
    INCLUDE([ImportCustomDataID], [FieldID], [FieldValue], [ReferenceID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ImportCustomData_FieldTypeID_FieldID_ReferenceID]
    ON [dbo].[ImportCustomData]([FieldTypeID] ASC, [FieldID] ASC, [ReferenceID] ASC)
    INCLUDE([ImportCustomDataID], [FieldValue]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

