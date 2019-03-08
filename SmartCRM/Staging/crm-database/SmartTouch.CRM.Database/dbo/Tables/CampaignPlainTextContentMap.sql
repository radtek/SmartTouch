CREATE TABLE [dbo].[CampaignPlainTextContentMap] (
    [CampaignPlainTextContentMapID] INT            IDENTITY (1, 1) NOT NULL,
    [CampaignID]                    INT            NOT NULL,
    [PlainTextContent]              NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_CampaignPlainTextContentMap] PRIMARY KEY CLUSTERED ([CampaignPlainTextContentMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignPlainTextContentMap_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID])
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignPlainTextContentMap_CampaignID]
    ON [dbo].[CampaignPlainTextContentMap]([CampaignID] ASC)
    INCLUDE([PlainTextContent]);

