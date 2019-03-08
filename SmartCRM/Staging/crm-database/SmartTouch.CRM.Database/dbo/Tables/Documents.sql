CREATE TABLE [dbo].[Documents] (
    [DocumentID]       INT             IDENTITY (1, 1) NOT NULL,
    [ContactID]        INT             NULL,
    [OriginalFileName] NVARCHAR (256)  NOT NULL,
    [StorageFileName]  NVARCHAR (75)   NOT NULL,
    [DocumentTypeID]   TINYINT         NOT NULL,
    [FileTypeID]       TINYINT         NOT NULL,
    [CreatedBy]        INT             NOT NULL,
    [CreatedDate]      DATETIME        NOT NULL,
    [ModifiedBy]       INT             NULL,
    [ModifiedDate]     DATETIME        NULL,
    [FilePath]         NVARCHAR (1000) NOT NULL,
    [OpportunityID]    INT             NULL,
    [StorageSource]    CHAR (1)        NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED ([DocumentID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Documents_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_Documents_DocumentTypes] FOREIGN KEY ([DocumentTypeID]) REFERENCES [dbo].[DocumentTypes] ([DocumentTypeID]),
    CONSTRAINT [FK_Documents_FileTypes] FOREIGN KEY ([FileTypeID]) REFERENCES [dbo].[FileTypes] ([FileTypeID]),
    CONSTRAINT [FK_Documents_Opportunities] FOREIGN KEY ([OpportunityID]) REFERENCES [dbo].[Opportunities] ([OpportunityID]),
    CONSTRAINT [FK_Documents_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Documents_Users1] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);

