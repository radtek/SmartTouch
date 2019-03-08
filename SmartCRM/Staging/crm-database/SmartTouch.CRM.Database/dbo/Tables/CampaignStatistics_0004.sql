CREATE TABLE [dbo].[CampaignStatistics_0004] (
    [CampaignTrackerID]   INT      CONSTRAINT [DF__CampaignS__Campa__5224FDE5] DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    CONSTRAINT [PK_CampaignStatistics_0004] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0004_CampaignID] CHECK ([CampaignID]>=(7501) AND [CampaignID]<=(10000))
);


GO
CREATE STATISTICS [_dta_stat_1874157772_7]
    ON [dbo].[CampaignStatistics_0004]([LinkIndex]);

