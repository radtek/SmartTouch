CREATE TABLE [dbo].[UserActivityLogs] (
    [UserActivityLogID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [EntityID]          INT            NOT NULL,
    [UserID]            INT            NULL,
    [ModuleID]          TINYINT        NOT NULL,
    [UserActivityID]    TINYINT        NOT NULL,
    [LogDate]           DATETIME       NOT NULL,
    [AccountID]         INT            NULL,
    [EntityName]        NVARCHAR (4000) NULL
    
);


GO


CREATE NONCLUSTERED INDEX [IX_UserActivityLogs_missing_15985] ON [dbo].[UserActivityLogs]
(
	[UserID] ASC,
	[ModuleID] ASC,
	[UserActivityID] ASC,
	[AccountID] ASC
)
INCLUDE ( 	[EntityID],
	[LogDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO

Create NonClustered Index IX_UserActivityLogs_missing_39 On [dbo].[UserActivityLogs] ([UserActivityLogID]);
GO


CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-UserActivityLogs] ON [dbo].[UserActivityLogs] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [AccountId_Scheme_UserActivityLogs]([AccountID])

GO
