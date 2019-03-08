CREATE TABLE [dbo].[ContactImages] (
    [ContactImageID] INT              IDENTITY (1, 1) NOT NULL,
    [OriginalName]   NVARCHAR (MAX)   NULL,
    [ImageContent]   NVARCHAR (MAX)   NULL,
    [ImageType]      NVARCHAR (15)    NULL,
    [StorageName]    UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_ContactImages] PRIMARY KEY CLUSTERED ([ContactImageID] ASC) WITH (FILLFACTOR = 90)
);

