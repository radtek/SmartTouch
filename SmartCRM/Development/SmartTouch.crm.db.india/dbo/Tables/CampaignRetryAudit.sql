CREATE TABLE [dbo].[CampaignRetryAudit](
	[CampaignRetryAuditID] [int] IDENTITY(1,1) NOT NULL,
	[CampaignID] [int] NOT NULL,
	[RetriedOn] [datetime] NOT NULL,
	[CampaignStatus] [smallint] NOT NULL,
	[Remarks] [nvarchar](1000) NULL,
CONSTRAINT [PK_CampaignRetryAudit] PRIMARY KEY CLUSTERED ([CampaignRetryAuditID] ASC) WITH (FILLFACTOR = 90) ON [Primary],
CONSTRAINT [FK_CampaignRetryAudit_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
CONSTRAINT [FK_CampaignRetryAudit_Statuses] FOREIGN KEY ([CampaignStatus]) REFERENCES [dbo].[Statuses] ([StatusID]));
GO


