CREATE TABLE [dbo].[CampaignAnalytics] (
    [CampaignStatisticsID] INT      IDENTITY (1, 1) NOT NULL,
    [CampaignID]           INT      DEFAULT ((0)) NULL,
    [Recipients]           INT      DEFAULT ((0)) NULL,
    [Sent]                 INT      DEFAULT ((0)) NULL,
    [Delivered]            INT      DEFAULT ((0)) NULL,
    [Bounced]              INT      DEFAULT ((0)) NULL,
    [Opened]               INT      DEFAULT ((0)) NULL,
    [Clicked]              INT      DEFAULT ((0)) NULL,
    [Complained]           INT      DEFAULT ((0)) NULL,
    [OptedOut]             INT      DEFAULT ((0)) NULL,
    [LastModifiedOn]       DATETIME DEFAULT ((0)) NULL,
    [Blocked]              INT      NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignAnalytics_CampaignID]
    ON [dbo].[CampaignAnalytics]([CampaignID] ASC)
    INCLUDE([Recipients], [Sent], [Delivered], [Opened], [Clicked], [Complained], [OptedOut]) WITH (FILLFACTOR = 90);

