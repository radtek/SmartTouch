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
CREATE STATISTICS [_dta_stat_1154155207_7]
    ON [dbo].[CampaignStatistics_0001]([LinkIndex]);

