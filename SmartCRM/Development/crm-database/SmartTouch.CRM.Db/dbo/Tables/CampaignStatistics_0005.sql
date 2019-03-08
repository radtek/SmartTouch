CREATE TABLE [dbo].[CampaignStatistics_0005] (
    [CampaignTrackerID]   INT      CONSTRAINT [DF__CampaignS__Campa__55F58EC9] DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    CONSTRAINT [PK_CampaignStatistics_0005] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0005_CampaignID] CHECK ([CampaignID]>=(10001) AND [CampaignID]<=(12500))
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_ActivityDate_0005]
    ON [dbo].[CampaignStatistics_0005]([ActivityDate] ASC)
    INCLUDE([ContactID], [CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_ActivityType_0005]
    ON [dbo].[CampaignStatistics_0005]([CampaignID] ASC, [ActivityType] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_CampaignLinkID_LinkIndex_0005]
    ON [dbo].[CampaignStatistics_0005]([CampaignID] ASC, [CampaignLinkID] ASC, [LinkIndex] ASC) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_0005]
    ON [dbo].[CampaignStatistics_0005]([CampaignRecipientID] ASC)
    INCLUDE([CampaignTrackerID], [CampaignID], [ActivityType], [CampaignLinkID], [ActivityDate], [LinkIndex]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_ActivityType_0005]
    ON [dbo].[CampaignStatistics_0005]([CampaignRecipientID] ASC)
    INCLUDE([ActivityType]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];

