CREATE TABLE [dbo].[CampaignTemplatesTest] (
    [CampaignTemplateID] INT            IDENTITY (1, 1) NOT NULL,
    [HTMLContent]        NVARCHAR (MAX) NULL,
    [Name]               NVARCHAR (75)  NOT NULL,
    [Type]               TINYINT        NOT NULL,
    [ThumbnailImage]     INT            NULL,
    [Description]        NVARCHAR (200) NULL,
    [Status]             SMALLINT       NULL,
    [AccountID]          INT            NULL,
    [CreatedBy]          INT            NULL,
    [CreatedOn]          DATETIME       NULL
);

