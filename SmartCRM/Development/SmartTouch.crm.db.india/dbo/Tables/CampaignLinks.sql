CREATE TABLE [dbo].[CampaignLinks] (
    [CampaignLinkID] INT            IDENTITY (1, 1) NOT NULL,
    [CampaignID]     INT            NOT NULL,
    [URL]            NVARCHAR (MAX) NOT NULL,
    [LinkIndex]      TINYINT        NOT NULL,
    [Name]           NVARCHAR (75)  NULL,
    CONSTRAINT [PK_CampaignLinks] PRIMARY KEY CLUSTERED ([CampaignLinkID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignLinks_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID])
);


GO

CREATE NONCLUSTERED INDEX [IX_CampaignLinks_CampaignID_Name]
    ON [dbo].[CampaignLinks]([CampaignID] ASC)
    INCLUDE([CampaignLinkID], [URL], [LinkIndex], [Name]) WITH (FILLFACTOR = 90);

