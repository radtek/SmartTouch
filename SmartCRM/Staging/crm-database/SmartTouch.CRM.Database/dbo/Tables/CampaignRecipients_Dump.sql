CREATE TABLE [dbo].[CampaignRecipients_Dump] (
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
    [ComplainedOn]        DATETIME         NULL
);

