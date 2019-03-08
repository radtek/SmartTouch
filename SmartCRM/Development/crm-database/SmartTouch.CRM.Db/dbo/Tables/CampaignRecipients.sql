CREATE TABLE [dbo].[CampaignRecipients] (
    [CampaignRecipientID] INT              IDENTITY(93814840,1) NOT NULL,
    [CampaignID]          INT              NOT NULL,
    [ContactID]           INT              NOT NULL,
    [CreatedDate]         DATETIME         NOT NULL,
    [To]                  NVARCHAR (256)   NOT NULL,
    [ScheduleTime]        DATETIME         NULL,
    [SentOn]              DATETIME         NULL,
    [GUID]                UNIQUEIDENTIFIER NULL,
    [DeliveryStatus]      SMALLINT         NULL,
    [DeliveredOn]         DATETIME         NULL,
    [Remarks]             NVARCHAR (4000)   NULL,
    [ServiceProviderID]   INT              NULL,
    [LastModifiedOn]      DATETIME         NULL,
    [OptOutStatus]        SMALLINT         NULL,
    [WorkflowID]          SMALLINT         NULL,
    [HasUnsubscribed]     BIT              NOT NULL,
    [UnsubscribedOn]      DATETIME         NULL,
    [HasComplained]       BIT              NOT NULL,
    [ComplainedOn]        DATETIME         NULL,
	[AccountID]           INT              NULL
);
go

Create NonClustered Index IX_CampaignRecipients_missing_1 On [dbo].[CampaignRecipients] ([CampaignRecipientID])
GO

Create NonClustered Index IX_CampaignRecipients_missing_110 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID])
GO

Create NonClustered Index IX_CampaignRecipients_missing_115 On [dbo].[CampaignRecipients] ([WorkflowID], [AccountId]) Include ([ContactID], [DeliveryStatus], [HasUnsubscribed])
GO

Create NonClustered Index IX_CampaignRecipients_missing_31 On [dbo].[CampaignRecipients] ([CampaignRecipientID], [CampaignID], [AccountId])
GO

Create NonClustered Index IX_CampaignRecipients_missing_29 On [dbo].[CampaignRecipients] ([CampaignID],[DeliveryStatus]) Include ([CampaignRecipientID], [ContactID], [WorkflowID], [AccountId])
GO

Create NonClustered Index IX_CampaignRecipients_missing_1728 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID])
GO

Create NonClustered Index IX_CampaignRecipients_missing_1730 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID])
GO

Create NonClustered Index IX_CampaignRecipients_missing_15371 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID]);
GO


CREATE NONCLUSTERED INDEX [CampaignLogDetails_CampaignRecipients_CampaignRecipientID] ON [dbo].[CampaignRecipients]
(
	[To] ASC,
	[WorkflowID] ASC
)
INCLUDE ( 	[CampaignRecipientID],
	[CampaignID],
	[ContactID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO


CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_ContactID_CampaignRecipientID_DeliveryStatus] ON [dbo].[CampaignRecipients]
(
	[ContactID] ASC,
	[To] ASC
)
INCLUDE ( 	[CampaignRecipientID],
	[DeliveryStatus]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO

CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_AccountId_CampaignRecipientID_ContactID] ON [dbo].[CampaignRecipients]
(
	[CampaignID] ASC,
	[AccountId] ASC
)
INCLUDE ( 	[CampaignRecipientID],
	[ContactID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO




CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-CampaignRecipients] ON [dbo].[CampaignRecipients] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [AccountId_Scheme_CampaignRecipients]([AccountID])
GO