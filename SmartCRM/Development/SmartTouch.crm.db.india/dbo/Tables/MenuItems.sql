CREATE TABLE [dbo].[MenuItems] (
    [MenuID]                SMALLINT       IDENTITY (1, 1) NOT NULL,
    [MenuCategoryID]        TINYINT        NOT NULL,
    [Name]                  NVARCHAR (150) NOT NULL,
    [ToolTip]               NVARCHAR (75)  NULL,
    [Area]                  NVARCHAR (75)  NULL,
    [Controller]            NVARCHAR (75)  NULL,
    [Action]                NVARCHAR (75)  NULL,
    [ContentPartialView]    NVARCHAR (75)  NULL,
    [CssClass]              NVARCHAR (75)  NULL,
    [ParentMenuID]          SMALLINT       NULL,
    [SortingID]             TINYINT        NOT NULL,
    [OpenAsMenuView]        BIT            NULL,
    [ModuleOperationsMapID] INT            NULL,
    [ModuleID]              TINYINT        NOT NULL,
    CONSTRAINT [PK_MenuItems] PRIMARY KEY CLUSTERED ([MenuID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Menu_MenuCategory] FOREIGN KEY ([MenuCategoryID]) REFERENCES [dbo].[MenuCategories] ([MenuCategoryID]),
    CONSTRAINT [FK_MenuItems_ModuleOperationsMap] FOREIGN KEY ([ModuleOperationsMapID]) REFERENCES [dbo].[ModuleOperationsMap] ([ModuleOperationsMapID]),
    CONSTRAINT [FK_MenuItems_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID])
);

