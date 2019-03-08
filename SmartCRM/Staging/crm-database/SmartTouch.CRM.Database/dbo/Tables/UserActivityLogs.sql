CREATE TABLE [dbo].[UserActivityLogs] (
    [UserActivityLogID] BIGINT          IDENTITY (1, 1) NOT NULL,
    [EntityID]          INT             NOT NULL,
    [UserID]            INT             NULL,
    [ModuleID]          TINYINT         NOT NULL,
    [UserActivityID]    TINYINT         NOT NULL,
    [LogDate]           DATETIME        NOT NULL,
    [AccountID]         INT             NULL,
    [EntityName]        NVARCHAR (4000) NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_UserActivityLogs_missing_39]
    ON [dbo].[UserActivityLogs]([UserActivityLogID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [UserActivityLogs_UserActivityID_AccountID_EntityID_UserID_ModuleID_LogDate]
    ON [dbo].[UserActivityLogs]([UserActivityID] ASC, [AccountID] ASC)
    INCLUDE([EntityID], [UserID], [ModuleID], [LogDate]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [UserActivityLogs_ModuleID_UserActivityID_AccountID]
    ON [dbo].[UserActivityLogs]([ModuleID] ASC, [UserActivityID] ASC, [AccountID] ASC)
    INCLUDE([EntityID], [UserID], [LogDate]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-UserActivityLogs]
    ON [dbo].[UserActivityLogs]
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);

