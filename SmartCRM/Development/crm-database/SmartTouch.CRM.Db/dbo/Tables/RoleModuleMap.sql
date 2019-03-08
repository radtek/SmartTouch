CREATE TABLE [dbo].[RoleModuleMap] (
    [RoleModuleMapID] INT      IDENTITY (1, 1) NOT NULL,
    [RoleID]          SMALLINT NOT NULL,
    [ModuleID]        TINYINT  NOT NULL,
    CONSTRAINT [PK_RoleModuleMap] PRIMARY KEY CLUSTERED ([RoleModuleMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_RoleModuleMap_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_RoleModuleMap_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Roles] ([RoleID])
);

