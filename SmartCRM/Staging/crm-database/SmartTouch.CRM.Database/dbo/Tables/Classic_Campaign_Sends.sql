CREATE TABLE [dbo].[Classic_Campaign_Sends] (
    [ID]                   INT            IDENTITY (1, 1) NOT NULL,
    [AccountName]          NVARCHAR (100) COLLATE Latin1_General_CI_AI NULL,
    [CampaignName]         NVARCHAR (100) COLLATE Latin1_General_CI_AI NULL,
    [LastCampaignSentDate] DATETIME       NULL,
    [VMTAName]             NVARCHAR (200) COLLATE Latin1_General_CI_AI NULL,
    [SearchDefinitionName] NVARCHAR (MAX) COLLATE Latin1_General_CI_AI NULL,
    [TagName]              NVARCHAR (MAX) COLLATE Latin1_General_CI_AI NULL
);

