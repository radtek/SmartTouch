
CREATE TABLE [dbo].[CampaignPlainTextContentMap](
	[CampaignPlainTextContentMapID] [int] IDENTITY(1,1) NOT NULL,
	[CampaignID] [int] NOT NULL,
	[PlainTextContent] [nvarchar](max) NOT NULL DEFAULT (''),
 CONSTRAINT [PK_CampaignPlainTextContentMap] PRIMARY KEY CLUSTERED (CampaignPlainTextContentMapID ASC) WITH (FILLFACTOR = 90),
  CONSTRAINT [FK_CampaignPlainTextContentMap_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID])
  )
GO




