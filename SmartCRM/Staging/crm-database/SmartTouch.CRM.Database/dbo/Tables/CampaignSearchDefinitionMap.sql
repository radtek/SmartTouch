CREATE TABLE [dbo].[CampaignSearchDefinitionMap] (
    [CampaignSearchDefinitionMapID] INT IDENTITY (1, 1) NOT NULL,
    [CampaignID]                    INT NOT NULL,
    [SearchDefinitionID]            INT NOT NULL,
    CONSTRAINT [PK_CampaignSearchDefinitionMap] PRIMARY KEY CLUSTERED ([CampaignSearchDefinitionMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignSearchDefinitionMap_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_CampaignSearchDefinitionMap_SearchDefinitions] FOREIGN KEY ([SearchDefinitionID]) REFERENCES [dbo].[SearchDefinitions] ([SearchDefinitionID])
);

