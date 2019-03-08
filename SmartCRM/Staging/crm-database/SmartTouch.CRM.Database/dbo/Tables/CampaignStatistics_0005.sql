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

