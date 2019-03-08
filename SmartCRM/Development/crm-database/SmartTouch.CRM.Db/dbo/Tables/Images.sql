CREATE TABLE [dbo].[Images] (
    [ImageID]         INT            IDENTITY (1, 1) NOT NULL,
    [FriendlyName]    NVARCHAR (MAX) NOT NULL,
    [StorageName]     VARCHAR (2000) NOT NULL,
    [OriginalName]    NVARCHAR (MAX) NULL,
    [CreatedBy]       INT            NOT NULL,
    [CreatedDate]     DATETIME       NOT NULL,
    [ImageCategoryID] TINYINT        NOT NULL,
    [AccountID]       INT            NULL,
    CONSTRAINT [PK_Images] PRIMARY KEY CLUSTERED ([ImageID] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_Images_AccountID]
    ON [dbo].[Images]([AccountID] ASC)
    INCLUDE([FriendlyName]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Images_ImageCategoryID_AccountID]
    ON [dbo].[Images]([ImageCategoryID] ASC, [AccountID] ASC)
    INCLUDE([ImageID], [FriendlyName], [StorageName], [OriginalName]) WITH (FILLFACTOR = 90);

