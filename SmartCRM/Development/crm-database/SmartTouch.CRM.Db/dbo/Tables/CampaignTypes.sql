CREATE TABLE [dbo].[CampaignTypes] (
    [CampaignTypeID] TINYINT       NOT NULL,
    [CampaignType]   NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([CampaignTypeID] ASC) WITH (FILLFACTOR = 90)
);

