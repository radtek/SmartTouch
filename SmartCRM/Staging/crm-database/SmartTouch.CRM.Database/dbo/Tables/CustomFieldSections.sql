CREATE TABLE [dbo].[CustomFieldSections] (
    [CustomFieldSectionID] INT           IDENTITY (1, 1) NOT NULL,
    [Name]                 NVARCHAR (75) NOT NULL,
    [StatusID]             SMALLINT      NOT NULL,
    [SortID]               TINYINT       NOT NULL,
    [TabID]                INT           NOT NULL,
    CONSTRAINT [PK_CustomFieldSections] PRIMARY KEY CLUSTERED ([CustomFieldSectionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CustomFieldSections_CustomFieldTabs] FOREIGN KEY ([TabID]) REFERENCES [dbo].[CustomFieldTabs] ([CustomFieldTabID]),
    CONSTRAINT [FK_CustomFieldSections_Statuses] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[Statuses] ([StatusID])
);

