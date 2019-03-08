CREATE TABLE [dbo].[Modules] (
    [ModuleID]   TINYINT       IDENTITY (1, 1) NOT NULL,
    [ModuleName] NVARCHAR (75) NOT NULL,
    [IsInternal] BIT           NOT NULL,
    [ParentID]   TINYINT       NOT NULL,
    CONSTRAINT [PK_Modules] PRIMARY KEY CLUSTERED ([ModuleID] ASC) WITH (FILLFACTOR = 90)
);

