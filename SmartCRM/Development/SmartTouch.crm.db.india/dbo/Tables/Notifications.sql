CREATE TABLE [dbo].[Notifications] (
    [NotificationID]   INT            IDENTITY (1, 1) NOT NULL,
    [EntityID]         INT            NULL,
    [Subject]          VARCHAR (200)  NULL,
    [Details]          VARCHAR (4000) NULL,
    [NotificationTime] DATETIME       NULL,
    [Status]           TINYINT        NOT NULL,
    [UserID]           INT            NULL,
    [ModuleID]         TINYINT        NOT NULL,
    [DownloadFile]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([NotificationID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Notifications_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Notifications_EntityID]
    ON [dbo].[Notifications]([EntityID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Notifications_Status_UserID]
    ON [dbo].[Notifications]([Status] ASC, [UserID] ASC)
    INCLUDE([NotificationID], [EntityID], [Subject], [Details], [NotificationTime], [ModuleID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Notifications_Status_UserID_ModuleID]
    ON [dbo].[Notifications]([Status] ASC, [UserID] ASC, [ModuleID] ASC)
    INCLUDE([NotificationID], [NotificationTime]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Notifications_UserID_ModuleID_NotificationTime]
    ON [dbo].[Notifications]([UserID] ASC, [ModuleID] ASC, [NotificationTime] ASC)
    INCLUDE([NotificationID], [EntityID], [Subject], [Details], [Status], [DownloadFile]) WITH (FILLFACTOR = 90);

