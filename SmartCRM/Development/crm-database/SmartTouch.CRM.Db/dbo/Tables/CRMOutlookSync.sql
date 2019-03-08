CREATE TABLE [dbo].[CRMOutlookSync] (
    [OutlookSyncID] INT           IDENTITY (1, 1) NOT NULL,
    [EntityID]      INT           NOT NULL,
    [OutlookKey]    VARCHAR (200) NULL,
    [SyncStatus]    SMALLINT      NOT NULL,
    [LastSyncDate]  DATETIME      NULL,
    [LastSyncedBy]  INT           NULL,
    [EntityType]    TINYINT       NOT NULL,
    CONSTRAINT [PK_ContactOutlookSync] PRIMARY KEY CLUSTERED ([OutlookSyncID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactOutlookSync_Modules] FOREIGN KEY ([EntityType]) REFERENCES [dbo].[Modules] ([ModuleID]),
    CONSTRAINT [FK_ContactOutlookSync_Statuses] FOREIGN KEY ([SyncStatus]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_ContactOutlookSync_Users] FOREIGN KEY ([LastSyncedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_CRMOutlookSync_EntityID_EntityType]
    ON [dbo].[CRMOutlookSync]([EntityID] ASC, [EntityType] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_CRMOutlookSync_EntityID_SyncStatus]
    ON [dbo].[CRMOutlookSync]([EntityID] ASC, [SyncStatus] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_CRMOutlookSync_EntityID_SyncStatus_EntityType]
    ON [dbo].[CRMOutlookSync]([EntityID] ASC, [SyncStatus] ASC, [EntityType] ASC)
    INCLUDE([OutlookKey]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_CRMOutlookSync_EntityID_SyncStatus_EntityType_OutlookKey]
    ON [dbo].[CRMOutlookSync]([EntityID] ASC, [SyncStatus] ASC, [EntityType] ASC, [OutlookKey] ASC) WITH (FILLFACTOR = 90);


GO





CREATE NONCLUSTERED INDEX [IX_CRMOutlookSync_SyncStatus_LastSyncedBy_EntityType]
    ON [dbo].[CRMOutlookSync]([SyncStatus] ASC, [LastSyncedBy] ASC, [EntityType] ASC)
    INCLUDE([EntityID]) WITH (FILLFACTOR = 90);
GO

CREATE NONCLUSTERED INDEX [IX_CRMOutlookSync_missing_199] ON [dbo].[CRMOutlookSync]
(
	[SyncStatus] ASC,
	[EntityType] ASC
)
INCLUDE ( 	[EntityID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO