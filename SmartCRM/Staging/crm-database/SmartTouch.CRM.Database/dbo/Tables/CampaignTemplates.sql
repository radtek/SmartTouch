﻿CREATE TABLE [dbo].[CampaignTemplates] (
    [CampaignTemplateID] INT            IDENTITY (1, 1) NOT NULL,
    [HTMLContent]        NVARCHAR (MAX) NULL,
    [Name]               NVARCHAR (75)  NOT NULL,
    [Type]               TINYINT        NOT NULL,
    [ThumbnailImage]     INT            NULL,
    [Description]        NVARCHAR (200) NULL,
    [Status]             SMALLINT       NULL,
    [AccountID]          INT            NULL,
    [CreatedBy]          INT            NULL,
    [CreatedOn]          DATETIME       NULL,
    CONSTRAINT [PK_CampaignTemplates] PRIMARY KEY CLUSTERED ([CampaignTemplateID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CampaignTemplates_Images] FOREIGN KEY ([ThumbnailImage]) REFERENCES [dbo].[Images] ([ImageID]),
    CONSTRAINT [FK_CampaignTemplates_Statuses] FOREIGN KEY ([Status]) REFERENCES [dbo].[Statuses] ([StatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_CampaignTemplates_Status]
    ON [dbo].[CampaignTemplates]([Status] ASC, [AccountID] ASC)
    INCLUDE([CampaignTemplateID], [Name], [Type], [ThumbnailImage]) WITH (FILLFACTOR = 90);

