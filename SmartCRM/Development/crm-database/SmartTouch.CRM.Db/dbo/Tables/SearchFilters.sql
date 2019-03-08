CREATE TABLE [dbo].[SearchFilters] (
    [SearchFilterID]        SMALLINT       IDENTITY (1, 1) NOT NULL,
    [FieldID]               INT            NULL,
    [SearchQualifierTypeID] SMALLINT       NOT NULL,
    [SearchText]            NVARCHAR (256) NULL,
    [SearchDefinitionID]    INT            NOT NULL,
    [DropdownValueID]       SMALLINT       NULL,
    CONSTRAINT [PK_SearchFilters] PRIMARY KEY CLUSTERED ([SearchFilterID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_SearchFilters_DropdownValues] FOREIGN KEY ([DropdownValueID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_SearchFilters_Fields] FOREIGN KEY ([FieldID]) REFERENCES [dbo].[Fields] ([FieldID]),
    CONSTRAINT [FK_SearchFilters_SearchDefinitions] FOREIGN KEY ([SearchDefinitionID]) REFERENCES [dbo].[SearchDefinitions] ([SearchDefinitionID]),
    CONSTRAINT [FK_SearchFilters_SearchQualifierTypes] FOREIGN KEY ([SearchQualifierTypeID]) REFERENCES [dbo].[SearchQualifierTypes] ([SearchQualifierTypeID])
);

