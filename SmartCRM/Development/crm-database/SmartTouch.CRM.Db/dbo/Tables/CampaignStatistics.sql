CREATE TABLE [dbo].[CampaignStatistics] (
    [CampaignTrackerID]   INT       IDENTITY(2536140,1) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NOT NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NOT NULL,
	[AccountID]           INT              NULL
    
);


GO
  







Create NonClustered Index IX_CampaignStatistics_missing_112 On [dbo].[CampaignStatistics] ([CampaignID], [ActivityType], [AccountId]) Include ([CampaignRecipientID]);
GO



Create NonClustered Index IX_CampaignStatistics_missing_15572 On [dbo].[CampaignStatistics] ([AccountId],[CampaignRecipientID]) Include ([CampaignTrackerID], [CampaignID], [ActivityType], [CampaignLinkID], [ActivityDate], [LinkIndex]);
GO

Create NonClustered Index IX_CampaignStatistics_AccountId_CampaignLinkID_CampaignRecipientID On [dbo].[CampaignStatistics] ([AccountId]) Include ([CampaignLinkID], [CampaignRecipientID]);
GO

CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-CampaignStatistics] ON [dbo].[CampaignStatistics] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [AccountId_Scheme_CampaignRecipients]([AccountID])

GO