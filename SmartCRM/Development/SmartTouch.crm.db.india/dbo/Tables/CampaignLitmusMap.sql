CREATE TABLE [dbo].[CampaignLitmusMap](
	[CampaignLitmusMapId] [int] IDENTITY(1,1) NOT NULL,
	[CampaignId] [int] NULL,
	[LitmusId] [varchar](100) NULL,
	[ProcessingStatus] [int] NULL,
	[CreatedOn] [datetime] NULL,
	[LastModifiedOn] [datetime] NULL,
	[Remarks] [varchar](250) NULL
) ON [PRIMARY]

GO