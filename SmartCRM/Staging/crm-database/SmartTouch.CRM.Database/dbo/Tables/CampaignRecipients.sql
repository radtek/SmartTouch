CREATE TABLE [dbo].[CampaignRecipients] (
    [CampaignRecipientID] INT              IDENTITY (75327588, 1) NOT NULL,
    [CampaignID]          INT              NOT NULL,
    [ContactID]           INT              NOT NULL,
    [CreatedDate]         DATETIME         NOT NULL,
    [To]                  NVARCHAR (256)   NOT NULL,
    [ScheduleTime]        DATETIME         NULL,
    [SentOn]              DATETIME         NULL,
    [GUID]                UNIQUEIDENTIFIER NULL,
    [DeliveredOn]         DATETIME         NULL,
    [DeliveryStatus]      SMALLINT         NULL,
    [LastModifiedOn]      DATETIME         NULL,
    [OptOutStatus]        SMALLINT         NULL,
    [Remarks]             NVARCHAR (4000)  NULL,
    [ServiceProviderID]   INT              NULL,
    [HasUnsubscribed]     BIT              NOT NULL,
    [UnsubscribedOn]      DATETIME         NULL,
    [HasComplained]       BIT              NOT NULL,
    [ComplainedOn]        DATETIME         NULL,
    [WorkflowID]          SMALLINT         NULL,
    [AccountId]           INT              NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_missing_1]
    ON [dbo].[CampaignRecipients]([CampaignRecipientID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_missing_110]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC)
    INCLUDE([CampaignRecipientID], [ContactID], [DeliveryStatus], [OptOutStatus], [HasComplained], [WorkflowID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_missing_115]
    ON [dbo].[CampaignRecipients]([WorkflowID] ASC, [AccountId] ASC)
    INCLUDE([ContactID], [DeliveryStatus], [HasUnsubscribed]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_missing_31]
    ON [dbo].[CampaignRecipients]([CampaignRecipientID] ASC, [CampaignID] ASC, [AccountId] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_missing_29]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC, [DeliveryStatus] ASC)
    INCLUDE([CampaignRecipientID], [ContactID], [WorkflowID], [AccountId]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_ContactID]
    ON [dbo].[CampaignRecipients]([ContactID] ASC, [To] ASC)
    INCLUDE([CampaignRecipientID], [DeliveryStatus]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_CampaignRecipientID]
    ON [dbo].[CampaignRecipients]([CampaignID] ASC)
    INCLUDE([CampaignRecipientID], [ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [CampaignRecipients_AccountId_SentOn_DeliveryStatus]
    ON [dbo].[CampaignRecipients]([AccountId] ASC, [SentOn] ASC, [DeliveryStatus] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_missing_681]
    ON [dbo].[CampaignRecipients]([ContactID] ASC, [AccountId] ASC)
    INCLUDE([CampaignRecipientID], [SentOn], [DeliveredOn], [DeliveryStatus]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE NONCLUSTERED INDEX [CampaignRecipients_DeliveredOn_AccountId]
    ON [dbo].[CampaignRecipients]([DeliveredOn] ASC, [AccountId] ASC)
    INCLUDE([CampaignRecipientID], [CampaignID], [ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [CampaignRecipients_ClusteredColumnStoreIndex]
    ON [dbo].[CampaignRecipients]
    ON [AccountId_Scheme_CampaignRecipients] ([AccountId]);

