CREATE TABLE [dbo].[UserActivityLogs] (
    [UserActivityLogID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [EntityID]          INT            NOT NULL,
    [UserID]            INT            NULL,
    [ModuleID]          TINYINT        NOT NULL,
    [UserActivityID]    TINYINT        NOT NULL,
    [LogDate]           DATETIME       NOT NULL,
    [AccountID]         INT            NULL,
    [EntityName]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserActivityLogs] PRIMARY KEY CLUSTERED ([UserActivityLogID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_UserActivityLogs_Modules] FOREIGN KEY ([ModuleID]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_UserActivityLogs_UserActivities] FOREIGN KEY ([UserActivityID]) REFERENCES [dbo].[UserActivities] ([UserActivityID])
) TEXTIMAGE_ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserActivityLogs_EntityID_ModuleID]
    ON [dbo].[UserActivityLogs]([EntityID] ASC, [ModuleID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserActivityLogs_UserID_AccountID_ModuleID]
    ON [dbo].[UserActivityLogs]([UserID] ASC, [AccountID] ASC, [ModuleID] ASC)
    INCLUDE([UserActivityLogID], [EntityID], [UserActivityID], [LogDate], [EntityName]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserActivityLogs_UserID_ModuleID_UserActivityID]
    ON [dbo].[UserActivityLogs]([UserID] ASC, [ModuleID] ASC, [UserActivityID] ASC)
    INCLUDE([EntityID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_UserActivityLogs_UserID_UserActivityID_AccountID]
    ON [dbo].[UserActivityLogs]([UserID] ASC, [UserActivityID] ASC, [AccountID] ASC)
    INCLUDE([ModuleID], [LogDate]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];

GO

Create NonClustered Index IX_UserActivityLogs_missing_39 On [dbo].[UserActivityLogs] ([UserActivityLogID]);
GO
