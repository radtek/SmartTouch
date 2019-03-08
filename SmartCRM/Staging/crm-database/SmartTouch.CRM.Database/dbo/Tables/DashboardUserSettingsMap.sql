CREATE TABLE [dbo].[DashboardUserSettingsMap] (
    [UserSettingsMapID] INT     IDENTITY (1, 1) NOT NULL,
    [UserID]            INT     NOT NULL,
    [DashBoardID]       TINYINT NOT NULL,
    CONSTRAINT [PK_DashboardUserSettingsMap] PRIMARY KEY CLUSTERED ([UserSettingsMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_DashboardUserSettingsMap_DashboardSettings] FOREIGN KEY ([DashBoardID]) REFERENCES [dbo].[DashboardSettings] ([DashBoardID]),
    CONSTRAINT [FK_DashboardUserSettingsMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

