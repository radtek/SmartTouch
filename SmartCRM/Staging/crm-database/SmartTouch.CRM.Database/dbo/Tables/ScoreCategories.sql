CREATE TABLE [dbo].[ScoreCategories] (
    [ScoreCategoryID] SMALLINT     IDENTITY (1, 1) NOT NULL,
    [Name]            VARCHAR (75) NOT NULL,
    [ModuleID]        TINYINT      NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([ScoreCategoryID] ASC) WITH (FILLFACTOR = 90)
);

