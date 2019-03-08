CREATE TABLE [dbo].[ImageCategories] (
    [ImageCategoryID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_ImageCategory] PRIMARY KEY CLUSTERED ([ImageCategoryID] ASC) WITH (FILLFACTOR = 90)
);

