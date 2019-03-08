CREATE TABLE [dbo].[CampaignContactTagMap] (
    [CampaignContactTagMapID] INT IDENTITY (1, 1) NOT NULL,
    [CampaignID]              INT NOT NULL,
    [TagID]                   INT NULL,
    CONSTRAINT [PK_CampaignContactTagMap] PRIMARY KEY CLUSTERED ([CampaignContactTagMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_CampaignContactTagMap_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_CampaignContactTagMap_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]) ON DELETE CASCADE
);

