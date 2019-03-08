CREATE TABLE [dbo].[Conditions] (
    [ConditionID]     TINYINT       IDENTITY (1, 1) NOT NULL,
    [Name]            VARCHAR (250) NULL,
    [ScoreCategoryID] SMALLINT      NOT NULL,
    [ModuleID]        TINYINT       NULL,
    CONSTRAINT [PK_Conditions] PRIMARY KEY CLUSTERED ([ConditionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Conditions_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_Conditions_ScoreCategories] FOREIGN KEY ([ScoreCategoryID]) REFERENCES [dbo].[ScoreCategories] ([ScoreCategoryID])
);

