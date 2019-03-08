CREATE TABLE [dbo].[DocumentTypes] (
    [DocumentTypeID]   TINYINT       IDENTITY (1, 1) NOT NULL,
    [DocumentTypeName] NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_DocumentTypes] PRIMARY KEY CLUSTERED ([DocumentTypeID] ASC) WITH (FILLFACTOR = 90)
);

