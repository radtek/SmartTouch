CREATE TABLE [dbo].[AccountDataAccessPermissions] (
    [AccountDataAccessPermissionID] INT     IDENTITY (1, 1) NOT NULL,
    [AccountID]                     INT     NULL,
    [ModuleID]                      TINYINT NOT NULL,
    [IsPrivate]                     BIT     NOT NULL,
    CONSTRAINT [PK_AccountDataAccessPermissions] PRIMARY KEY CLUSTERED ([AccountDataAccessPermissionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_AccountDataAccessPermissions_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_AccountDataAccessPermissions_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID])
);

