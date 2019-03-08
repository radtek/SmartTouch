CREATE TABLE [dbo].[CampaignStatistics_0002] (
    [CampaignTrackerID]   INT      DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    CONSTRAINT [PK_CampaignStatistics_0002] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0002_CampaignID] CHECK ([CampaignID]>=(2501) AND [CampaignID]<=(5000))
);


GO
CREATE STATISTICS [_dta_stat_1218155435_7]
    ON [dbo].[CampaignStatistics_0002]([LinkIndex]);

