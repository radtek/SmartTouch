CREATE TABLE [dbo].[CampaignLogDetails] (
    [CampaignLogDetailsID] INT            IDENTITY (1, 1) NOT NULL,
    [CampaignId]           INT            NULL,
    [CampaignRecipientId]  INT            NULL,
    [Recipient]            NVARCHAR (MAX) NULL,
    [DeliveryStatus]       SMALLINT       NULL,
    [OptOutStatus]         SMALLINT       NULL,
    [BounceCategory]       INT            NULL,
    [TimeLogged]           DATETIME       NULL,
    [Remarks]              NVARCHAR (MAX) NULL,
    [Status]               TINYINT        NULL,
    [FileType]             INT            NULL,
    [CreatedOn]            DATETIME       NOT NULL,
    CONSTRAINT [PK_CampaignLogDetails] PRIMARY KEY CLUSTERED ([CampaignLogDetailsID] ASC) WITH (FILLFACTOR = 90)
);

