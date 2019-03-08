CREATE TABLE [dbo].[CampaignRecipients] (
    [CampaignRecipientID] INT              IDENTITY (1, 1) NOT NULL,
    [CampaignID]          INT              NOT NULL,
    [ContactID]           INT              NOT NULL,
    [CreatedDate]         DATETIME         NOT NULL,
    [To]                  NVARCHAR (256)   NOT NULL,
    [ScheduleTime]        DATETIME         NULL,
    [SentOn]              DATETIME         NULL,
    [GUID]                UNIQUEIDENTIFIER NULL,
    [DeliveryStatus]      SMALLINT         NULL,
    [DeliveredOn]         DATETIME         NULL,
    [Remarks]             NVARCHAR (MAX)   NULL,
    [ServiceProviderID]   INT              NULL,
    [LastModifiedOn]      DATETIME         NULL,
    [OptOutStatus]        SMALLINT         NULL,
    [WorkflowID]          SMALLINT         NULL,
    [HasUnsubscribed]     BIT              NOT NULL,
    [UnsubscribedOn]      DATETIME         NULL,
    [HasComplained]       BIT              NOT NULL,
    [ComplainedOn]        DATETIME         NULL,
	[AccountID]           INT              NULL,
    CONSTRAINT [PK_CampaignRecipients] PRIMARY KEY CLUSTERED ([CampaignRecipientID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_CampaignRecipients_Campaigns] FOREIGN KEY ([CampaignID]) REFERENCES [dbo].[Campaigns] ([CampaignID]),
    CONSTRAINT [FK_CampaignRecipients_ServiceProviders] FOREIGN KEY ([ServiceProviderID]) REFERENCES [dbo].[ServiceProviders] ([ServiceProviderID]),
    CONSTRAINT [FK_CampaignRecipients_Statuses] FOREIGN KEY ([DeliveryStatus]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_CampaignRecipients_Statuses1] FOREIGN KEY ([OptOutStatus]) REFERENCES [dbo].[Statuses] ([StatusID]),
    CONSTRAINT [FK_CampaignRecipients_Workflows] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflows] ([WorkflowID])
) TEXTIMAGE_ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_DeliveryStatus_ContactID]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [DeliveryStatus] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_DeliveryStatus_WorkflowID_DeliveredOn]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [DeliveryStatus] ASC, [WorkflowID] ASC, [DeliveredOn] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_HasComplained]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [HasComplained] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_HasUnsubscribed]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [HasUnsubscribed] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_SentOn_CampaignRecipientID]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [SentOn] ASC)
    INCLUDE([CampaignRecipientID], [DeliveryStatus], [HasComplained]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_To_CampaignStatusID]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [To] ASC)
    INCLUDE([CampaignRecipientID], [ContactID], [CreatedDate], [ScheduleTime], [SentOn], [GUID], [DeliveryStatus], [DeliveredOn], [Remarks], [ServiceProviderID], [LastModifiedOn], [OptOutStatus], [WorkflowID], [HasUnsubscribed], [UnsubscribedOn], [HasComplained], [ComplainedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_WorkflowID_HasComplained]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [WorkflowID] ASC, [HasComplained] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_ContactID]
    ON [dbo].[CampaignRecipients]([ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_LastModifiedOn]
    ON [dbo].[CampaignRecipients]([LastModifiedOn] ASC)
    INCLUDE([ContactID], [To]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_To_DeliveryStatus_LastModifiedOn]
    ON [dbo].[CampaignRecipients]([To] ASC, [DeliveryStatus] ASC)
    INCLUDE([LastModifiedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_WorkflowID]
    ON [dbo].[CampaignRecipients]([WorkflowID] ASC)
    INCLUDE([DeliveryStatus], [HasUnsubscribed]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO

Create NonClustered Index IX_CampaignRecipients_missing_1 On [dbo].[CampaignRecipients] ([CampaignRecipientID]);
GO

Create NonClustered Index IX_CampaignRecipients_missing_110 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID]);
GO

Create NonClustered Index IX_CampaignRecipients_missing_115 On [dbo].[CampaignRecipients] ([WorkflowID], [AccountId]) Include ([ContactID], [DeliveryStatus], [HasUnsubscribed]);
GO

Create NonClustered Index IX_CampaignRecipients_missing_31 On [dbo].[CampaignRecipients] ([CampaignRecipientID], [CampaignID], [AccountId]);
GO

Create NonClustered Index IX_CampaignRecipients_missing_29 On [dbo].[CampaignRecipients] ([CampaignID],[DeliveryStatus]) Include ([CampaignRecipientID], [ContactID], [WorkflowID], [AccountId]);
GO

Create NonClustered Index IX_CampaignRecipients_missing_1728 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID]);
GO

Create NonClustered Index IX_CampaignRecipients_missing_1730 On [dbo].[CampaignRecipients] ([CampaignID]) Include ([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID]);
GO


