CREATE TABLE [dbo].[CampaignTagMap] (
    [CampaignTagMapID] INT IDENTITY (1, 1) NOT NULL,
    [CampaignID]       INT NOT NULL,
    [TagID]            INT NOT NULL,
    CONSTRAINT [PK_CampaignTagMap] PRIMARY KEY CLUSTERED ([CampaignTagMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignTagMap_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_CampaignTagMap_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]) ON DELETE CASCADE
);

