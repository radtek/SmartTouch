CREATE TABLE [dbo].[MenuCategories] (
    [MenuCategoryID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_MenuCategories] PRIMARY KEY CLUSTERED ([MenuCategoryID] ASC) WITH (FILLFACTOR = 90)
);

