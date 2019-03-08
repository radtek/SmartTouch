CREATE TABLE [dbo].[CampaignStatistics_0001] (
    [CampaignTrackerID]   INT      DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    CONSTRAINT [PK_CampaignStatistics_0001] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0001_CampaignID] CHECK ([CampaignID]>=(1) AND [CampaignID]<=(2500))
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_ActivityDate_0001]
    ON [dbo].[CampaignStatistics_0001]([ActivityDate] ASC)
    INCLUDE([ContactID], [CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_ActivityType_0001]
    ON [dbo].[CampaignStatistics_0001]([CampaignID] ASC, [ActivityType] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_CampaignLinkID_LinkIndex_0001]
    ON [dbo].[CampaignStatistics_0001]([CampaignID] ASC, [CampaignLinkID] ASC, [LinkIndex] ASC) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_0001]
    ON [dbo].[CampaignStatistics_0001]([CampaignRecipientID] ASC)
    INCLUDE([CampaignTrackerID], [CampaignID], [ActivityType], [CampaignLinkID], [ActivityDate], [LinkIndex]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_ActivityType_0001]
    ON [dbo].[CampaignStatistics_0001]([CampaignRecipientID] ASC)
    INCLUDE([ActivityType]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];

