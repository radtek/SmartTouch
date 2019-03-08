CREATE TABLE [dbo].[DocRepositorys] (
    [DocumentID]       BIGINT         IDENTITY (1, 1) NOT NULL,
    [ContactID]        INT            NOT NULL,
    [OriginalFileName] NVARCHAR (75)  NULL,
    [StorageFileName]  NVARCHAR (75)  NULL,
    [DocumentTypeID]   TINYINT        NOT NULL,
    [FileTypeID]       TINYINT        NOT NULL,
    [CreatedBy]        INT            NULL,
    [CreatedDate]      DATETIME       NULL,
    [ModifiedBy]       INT            NULL,
    [ModifiedDate]     DATETIME       NULL,
    [FilePath]         NVARCHAR (300) NULL,
    CONSTRAINT [PK_DocRepository] PRIMARY KEY CLUSTERED ([DocumentID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_DocRepository_DocumentTypes] FOREIGN KEY ([DocumentTypeID]) REFERENCES [dbo].[DocumentTypes] ([DocumentTypeID]),
    CONSTRAINT [FK_DocRepository_FileTypes] FOREIGN KEY ([FileTypeID]) REFERENCES [dbo].[FileTypes] ([FileTypeID]),
    CONSTRAINT [FK_DocRepositorys_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);

