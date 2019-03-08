CREATE TABLE [dbo].[CampaignStatistics_0004] (
    [CampaignTrackerID]   INT      CONSTRAINT [DF__CampaignS__Campa__5224FDE5] DEFAULT (NEXT VALUE FOR [CampaignStatisticsSequence]) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
	[AccountID]           INT              NULL,
    CONSTRAINT [PK_CampaignStatistics_0004] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG],
    CONSTRAINT [CampaignStatistics_0004_CampaignID] CHECK ([CampaignID]>=(7501) AND [CampaignID]<=(10000))
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_0004_CampaignLinkID]
    ON [dbo].[CampaignStatistics_0004]([CampaignLinkID] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_ActivityDate_0004]
    ON [dbo].[CampaignStatistics_0004]([ActivityDate] ASC)
    INCLUDE([ContactID], [CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID_ActivityType_0004]
    ON [dbo].[CampaignStatistics_0004]([CampaignID] ASC, [ActivityType] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_0004]
    ON [dbo].[CampaignStatistics_0004]([CampaignRecipientID] ASC)
    INCLUDE([CampaignTrackerID], [CampaignID], [ActivityType], [CampaignLinkID], [ActivityDate], [LinkIndex]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignRecipientID_ActivityType_0004]
    ON [dbo].[CampaignStatistics_0004]([CampaignRecipientID] ASC)
    INCLUDE([ActivityType]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];

