CREATE TABLE [dbo].[WebVisitUserNotificationMap] (
    [WebVisitUserNotificationMapID] INT      IDENTITY (1, 1) NOT NULL,
    [WebAnalyticsProviderID]        SMALLINT NOT NULL,
    [UserID]                        INT      NOT NULL,
    [AccountID]                     INT      NOT NULL,
    [NotificationType]              SMALLINT NOT NULL,
    [CreatedOn]                     DATETIME NOT NULL,
    [CreatedBy]                     INT      NOT NULL,
    [LastUpdatedOn]                 DATETIME NULL,
    [LastUpdatedBy]                 INT      NULL,
    CONSTRAINT [PK_WebVisitUserNotificationMap_1] PRIMARY KEY CLUSTERED ([WebVisitUserNotificationMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WebVisitUserNotificationMap_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_WebVisitUserNotificationMap_Statuses] FOREIGN KEY ([NotificationType]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_WebVisitUserNotificationMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_WebVisitUserNotificationMap_Users1] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_WebVisitUserNotificationMap_Users2] FOREIGN KEY ([LastUpdatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_WebVisitUserNotificationMap_WebAnalyticsProviders] FOREIGN KEY ([WebAnalyticsProviderID]) REFERENCES [dbo].[WebAnalyticsProviders] ([WebAnalyticsProviderID])
);

