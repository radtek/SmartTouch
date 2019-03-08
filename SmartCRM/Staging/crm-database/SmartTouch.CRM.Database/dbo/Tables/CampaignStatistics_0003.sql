CREATE TABLE [dbo].[CampaignStatistics_0003] (
    [CampaignTrackerID]   INT      DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    CONSTRAINT [PK_CampaignStatistics_0003] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0003_CampaignID] CHECK ([CampaignID]>=(5001) AND [CampaignID]<=(7500))
);

