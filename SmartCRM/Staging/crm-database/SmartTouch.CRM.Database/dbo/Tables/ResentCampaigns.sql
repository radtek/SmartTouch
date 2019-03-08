CREATE TABLE [dbo].[ResentCampaigns] (
    [ResentCampaignID]   INT      IDENTITY (1, 1) NOT NULL,
    [ParentCampaignID]   INT      NOT NULL,
    [CampaignID]         INT      NOT NULL,
    [CampaignResentTo]   SMALLINT NULL,
    [CamapignResentDate] DATETIME NULL,
    CONSTRAINT [PK_ResentCampaigns] PRIMARY KEY CLUSTERED ([ResentCampaignID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ResentCampaigns_Campaigns] FOREIGN KEY ([ParentCampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_ResentCampaigns_Campaigns1] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_ResentCampaigns_Statuses] FOREIGN KEY ([CampaignResentTo]) REFERENCES [dbo].[Statuses] ([StatusID])
);

