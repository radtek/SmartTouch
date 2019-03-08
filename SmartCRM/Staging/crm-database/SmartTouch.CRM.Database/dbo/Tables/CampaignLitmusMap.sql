CREATE TABLE [dbo].[CampaignLitmusMap] (
    [CampaignLitmusMapId] INT           IDENTITY (1, 1) NOT NULL,
    [CampaignId]          INT           NULL,
    [LitmusId]            VARCHAR (100) NULL,
    [ProcessingStatus]    INT           NULL,
    [CreatedOn]           DATETIME      NULL,
    [LastModifiedOn]      DATETIME      NULL,
    [Remarks]             VARCHAR (250) NULL
);

