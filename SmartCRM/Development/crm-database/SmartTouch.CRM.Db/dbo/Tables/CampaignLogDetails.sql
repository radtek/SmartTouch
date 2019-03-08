CREATE TABLE [dbo].[CampaignLogDetails] (
    [CampaignLogDetailsID] INT            IDENTITY (1, 1) NOT NULL,
    [CampaignId]           INT            NULL,
    [CampaignRecipientId]  INT            NULL,
    [Recipient]            NVARCHAR (4000) NULL,
    [DeliveryStatus]       SMALLINT       NULL,
    [OptOutStatus]         SMALLINT       NULL,
    [BounceCategory]       INT            NULL,
    [TimeLogged]           DATETIME       NULL,
    [Remarks]              NVARCHAR (4000) NULL,
    [Status]               TINYINT        NULL,
    [FileType]             INT            NULL,
    [CreatedOn]            DATETIME       NOT NULL
    
);
go

CREATE NONCLUSTERED INDEX [NonClusteredIndex-Recipient] ON [dbo].[CampaignLogDetails]
(
	[Recipient] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [NonClusteredIndex-CampaignId] ON [dbo].[CampaignLogDetails]
(
	[CampaignId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [ClusteredIndex-CampaignLogDetailsID] ON [dbo].[CampaignLogDetails]
(
	[CampaignLogDetailsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO


Create NonClustered Index IX_CampaignLogDetails_Status On [dbo].[CampaignLogDetails] ([Status]);
GO