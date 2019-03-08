CREATE TABLE [dbo].[CampaignMailStatus] (
    [Id]                 INT      IDENTITY (1, 1) NOT NULL,
    [CampaignId]         INT      NOT NULL,
    [CampaignSentStatus] BIT      NOT NULL,
    [CreatedDate]        DATETIME NOT NULL,
    CONSTRAINT [PK_CampaignMailStatus] PRIMARY KEY CLUSTERED ([Id] ASC)
);

