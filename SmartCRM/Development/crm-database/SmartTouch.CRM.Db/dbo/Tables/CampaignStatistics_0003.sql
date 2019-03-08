CREATE TABLE [dbo].[CampaignStatistics_0003] (
    [CampaignTrackerID]   INT      DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    CONSTRAINT [PK_CampaignStatistics_0003] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0003_CampaignID] CHECK ([CampaignID]>=(5001) AND [CampaignID]<=(7500))
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_0003_CampaignLinkID]
    ON [dbo].[CampaignStatistics_0003]([CampaignLinkID] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_ActivityDate_0003]
    ON [dbo].[CampaignStatistics_0003]([ActivityDate] ASC)
    INCLUDE([ContactID], [CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_ActivityType_0003]
    ON [dbo].[CampaignStatistics_0003]([CampaignID] ASC, [ActivityType] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_0003]
    ON [dbo].[CampaignStatistics_0003]([CampaignRecipientID] ASC)
    INCLUDE([CampaignTrackerID], [CampaignID], [ActivityType], [CampaignLinkID], [ActivityDate], [LinkIndex]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_ActivityType_0003]
    ON [dbo].[CampaignStatistics_0003]([CampaignRecipientID] ASC)
    INCLUDE([ActivityType]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];

