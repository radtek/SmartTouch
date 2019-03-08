CREATE TABLE [dbo].[CampaignLogDetails] (
    [CampaignLogDetailsID] INT             IDENTITY (1, 1) NOT NULL,
    [CampaignId]           INT             NULL,
    [CampaignRecipientId]  INT             NULL,
    [Recipient]            NVARCHAR (4000) NULL,
    [DeliveryStatus]       SMALLINT        NULL,
    [OptOutStatus]         SMALLINT        NULL,
    [BounceCategory]       INT             NULL,
    [TimeLogged]           DATETIME        NULL,
    [Remarks]              NVARCHAR (4000) NULL,
    [Status]               TINYINT         NULL,
    [FileType]             INT             NULL,
    [CreatedOn]            DATETIME        NOT NULL
);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-CampaignLogDetails]
    ON [dbo].[CampaignLogDetails];

