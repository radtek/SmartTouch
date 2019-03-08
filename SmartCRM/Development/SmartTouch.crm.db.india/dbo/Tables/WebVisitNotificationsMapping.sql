CREATE TABLE [dbo].[WebVisitNotificationsMapping] (
    [WebVisitNotificationsMappingID] INT     NOT NULL,
    [WebVisitID]                     INT     NOT NULL,
    [NotificationStatus]             TINYINT NOT NULL,
    CONSTRAINT [PK_WebVisitNotificationsMapping] PRIMARY KEY CLUSTERED ([WebVisitNotificationsMappingID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WebVisitNotificationsMapping_ContactWebVisits] FOREIGN KEY ([WebVisitID]) REFERENCES [dbo].[ContactWebVisits] ([ContactWebVisitID])
);

