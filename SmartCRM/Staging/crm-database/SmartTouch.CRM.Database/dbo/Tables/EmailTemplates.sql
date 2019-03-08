CREATE TABLE [dbo].[EmailTemplates] (
    [EmailTemplateID] INT          IDENTITY (1, 1) NOT NULL,
    [TemplateName]    VARCHAR (75) NOT NULL,
    CONSTRAINT [PK_EmailTemplates] PRIMARY KEY CLUSTERED ([EmailTemplateID] ASC) WITH (FILLFACTOR = 90)
);

