CREATE TABLE [dbo].[CampaignStatistics] (
    [CampaignTrackerID]   INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NULL,
	[AccountID]           INT              NULL,
    CONSTRAINT [PK_CampaignStatistics] PRIMARY KEY CLUSTERED ([CampaignTrackerID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignStatistics_CampaignLinks] FOREIGN KEY ([CampaignLinkID]) REFERENCES [dbo].[CampaignLinks] ([CampaignLinkID]),
    CONSTRAINT [FK_CampaignStatistics_CampaignRecipients] FOREIGN KEY ([CampaignRecipientID]) REFERENCES [dbo].[CampaignRecipients] ([CampaignRecipientID])
);


GO





CREATE NONCLUSTERED INDEX [IX_CampaignStatistics_CampaignLinkID]
    ON [dbo].[CampaignStatistics]([CampaignLinkID] ASC) WITH (FILLFACTOR = 90);


GO


Create NonClustered Index IX_CampaignStatistics_missing_112 On [dbo].[CampaignStatistics] ([CampaignID], [ActivityType], [AccountId]) Include ([CampaignRecipientID]);
GO

