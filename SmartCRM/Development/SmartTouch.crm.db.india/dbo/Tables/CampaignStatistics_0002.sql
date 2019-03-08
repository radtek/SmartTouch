CREATE TABLE [dbo].[CampaignStatistics_0002] (
    [CampaignTrackerID]   INT      DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
	[AccountID]           INT              NULL,
    CONSTRAINT [PK_CampaignStatistics_0002] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0002_CampaignID] CHECK ([CampaignID]>=(2501) AND [CampaignID]<=(5000))
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_0002_CampaignLinkID]
    ON [dbo].[CampaignStatistics_0002]([CampaignLinkID] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_ActivityDate_0002]
    ON [dbo].[CampaignStatistics_0002]([ActivityDate] ASC)
    INCLUDE([ContactID], [CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_ActivityType_0002]
    ON [dbo].[CampaignStatistics_0002]([CampaignID] ASC, [ActivityType] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_0002]
    ON [dbo].[CampaignStatistics_0002]([CampaignRecipientID] ASC)
    INCLUDE([CampaignTrackerID], [CampaignID], [ActivityType], [CampaignLinkID], [ActivityDate], [LinkIndex]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_ActivityType_0002]
    ON [dbo].[CampaignStatistics_0002]([CampaignRecipientID] ASC)
    INCLUDE([ActivityType]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];

