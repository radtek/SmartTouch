CREATE TABLE [dbo].[CampaignStatistics_Dump] (
    [CampaignTrackerID]   INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]           INT      NULL,
    [CampaignID]          INT      NULL,
    [ActivityType]        TINYINT  NOT NULL,
    [CampaignLinkID]      INT      NULL,
    [ActivityDate]        DATETIME NOT NULL,
    [LinkIndex]           TINYINT  NULL,
    [CampaignRecipientID] INT      NULL
);

