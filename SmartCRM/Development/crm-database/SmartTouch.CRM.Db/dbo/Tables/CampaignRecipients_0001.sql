﻿CREATE TABLE [dbo].[CampaignRecipients_0001] (
    [CampaignRecipientID] INT              NOT NULL,
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
    [Remarks]             NVARCHAR (MAX)   NULL,
    [ServiceProviderID]   INT              NULL,
    [HasUnsubscribed]     BIT              NOT NULL,
    [UnsubscribedOn]      DATETIME         NULL,
    [HasComplained]       BIT              NOT NULL,
    [ComplainedOn]        DATETIME         NULL,
    [WorkflowID]          SMALLINT         NULL,
    CONSTRAINT [PK_CampaignRecipients_0001] PRIMARY KEY CLUSTERED ([CampaignRecipientID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG],
    CONSTRAINT [CampaignRecipients_0001_CampaignID] CHECK ([CampaignID]>=(1) AND [CampaignID]<=(2500)),
    CONSTRAINT [UNIQUE_CampaignRecipients_0001_CampaignRecipientID_CampaignID] UNIQUE NONCLUSTERED ([CampaignRecipientID] ASC, [CampaignID] ASC) WITH (FILLFACTOR = 90) ON [SecondaryFG]
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_DeliveryStatus_ContactID_0001]
    ON [dbo].[CampaignRecipients_0001]([CampaignID] ASC, [DeliveryStatus] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO

CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_SentOn_CampaignRecipientID_0001]
    ON [dbo].[CampaignRecipients_0001]([CampaignID] ASC, [SentOn] ASC)
    INCLUDE([CampaignRecipientID], [DeliveryStatus], [HasComplained]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_To_CampaignStatusID_0001]
    ON [dbo].[CampaignRecipients_0001]([CampaignID] ASC, [To] ASC)
    INCLUDE([CampaignRecipientID], [ContactID], [CreatedDate], [ScheduleTime], [SentOn], [GUID], [DeliveryStatus], [DeliveredOn], [Remarks], [ServiceProviderID], [LastModifiedOn], [OptOutStatus], [WorkflowID], [HasUnsubscribed], [UnsubscribedOn], [HasComplained], [ComplainedOn]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_CampaignID_WorkflowID_HasComplained_0001]
    ON [dbo].[CampaignRecipients_0001]([CampaignID] ASC, [WorkflowID] ASC, [HasComplained] ASC) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_ContactID_0001]
    ON [dbo].[CampaignRecipients_0001]([ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_LastModifiedOn_0001]
    ON [dbo].[CampaignRecipients_0001]([LastModifiedOn] ASC)
    INCLUDE([ContactID], [To]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_To_DeliveryStatus_LastModifiedOn_0001]
    ON [dbo].[CampaignRecipients_0001]([To] ASC, [DeliveryStatus] ASC)
    INCLUDE([LastModifiedOn]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];


GO
CREATE NONCLUSTERED INDEX [IX_CampaignRecipients_WorkflowID_0001]
    ON [dbo].[CampaignRecipients_0001]([WorkflowID] ASC)
    INCLUDE([DeliveryStatus], [HasUnsubscribed]) WITH (FILLFACTOR = 90)
    ON [SecondaryFG];

