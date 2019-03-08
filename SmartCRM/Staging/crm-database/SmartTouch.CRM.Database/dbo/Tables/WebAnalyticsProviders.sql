CREATE TABLE [dbo].[WebAnalyticsProviders] (
    [WebAnalyticsProviderID]      SMALLINT      IDENTITY (1, 1) NOT NULL,
    [AccountID]                   INT           NOT NULL,
    [StatusID]                    SMALLINT      NOT NULL,
    [APIKey]                      VARCHAR (200) NOT NULL,
    [CreatedBy]                   INT           NOT NULL,
    [CreatedOn]                   DATETIME      NOT NULL,
    [LastUpdatedBy]               INT           NOT NULL,
    [LastUpdatedOn]               DATETIME      NOT NULL,
    [NotificationStatus]          BIT           NOT NULL,
    [NotificationFrequencyStatus] SMALLINT      NULL,
    [LastAPICallTimeStamp]        DATETIME      NULL,
    [ActivatedOn]                 DATETIME      NULL,
    [RequestInterval]             SMALLINT      NOT NULL,
    [TrackingDomain]              VARCHAR (300) NULL,
    [DailyStatusEmailOpted]       BIT           NOT NULL,
    CONSTRAINT [PK_WebAnalyticsProviders] PRIMARY KEY CLUSTERED ([WebAnalyticsProviderID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WebAnalyticsProviders_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_WebAnalyticsProviders_Statuses] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_WebAnalyticsProviders_Statuses1] FOREIGN KEY ([NotificationFrequencyStatus]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_WebAnalyticsProviders_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_WebAnalyticsProviders_Users1] FOREIGN KEY ([LastUpdatedBy]) REFERENCES [dbo].[Users] ([UserID])
);

