CREATE TABLE [dbo].[CampaignStatistics] (
    [CampaignTrackerID]   INT      IDENTITY (1179127, 1) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
    [AccountId]           INT      NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_missing_112]
    ON [dbo].[CampaignStatistics]([CampaignID] ASC, [ActivityType] ASC, [AccountId] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignID]
    ON [dbo].[CampaignStatistics]([CampaignID] ASC, [ActivityType] ASC, [AccountId] ASC, [ActivityDate] ASC)
    INCLUDE([CampaignRecipientID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [CampaignStatistics_ClusteredColumnStoreIndex]
    ON [dbo].[CampaignStatistics]
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);

