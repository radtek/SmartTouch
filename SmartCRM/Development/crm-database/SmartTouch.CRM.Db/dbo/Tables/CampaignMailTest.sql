CREATE TABLE [dbo].[CampaignMailTest](
	[CampaignMailTestID] [int] IDENTITY(1,1) NOT NULL,
	[CampaignID] [int] NOT NULL,
	[UniqueID] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NULL,
	[RawData] [nvarchar](max) NULL,
	[CreatedBy] [int] NOT NULL,
	CONSTRAINT [PK_CampaignMailTest] PRIMARY KEY CLUSTERED ([CampaignMailTestID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignMailTest_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
	CONSTRAINT [FK_CampaignMailTest_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID])

	)

GO

