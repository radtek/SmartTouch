CREATE TABLE [dbo].[RefreshAnalytics] (
    [RefreshAnalyticsID] INT      IDENTITY (1, 1) NOT NULL,
    [EntityID]           INT      DEFAULT ((0)) NULL,
    [EntityType]         TINYINT  DEFAULT ((0)) NULL,
    [Status]             TINYINT  DEFAULT ((1)) NULL,
    [LastModifiedOn]     DATETIME DEFAULT (getutcdate()) NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_RefreshAnalytics_Status]
    ON [dbo].[RefreshAnalytics]([Status] ASC)
    INCLUDE([EntityID], [EntityType]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_RefreshAnalytics_RefreshAnalyticsID]
    ON [dbo].[RefreshAnalytics]([RefreshAnalyticsID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

