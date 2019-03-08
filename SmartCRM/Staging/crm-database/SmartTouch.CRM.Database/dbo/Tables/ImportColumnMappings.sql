CREATE TABLE [dbo].[ImportColumnMappings] (
    [ImportColumnMappingID] INT           IDENTITY (1, 1) NOT NULL,
    [SheetColumnName]       VARCHAR (125) NULL,
    [IsCustomField]         BIT           NULL,
    [IsDropDownField]       BIT           NULL,
    [ContactFieldName]      VARCHAR (75)  NULL,
    [JobID]                 INT           NOT NULL,
    CONSTRAINT [PK_ImportColumnMappings] PRIMARY KEY CLUSTERED ([ImportColumnMappingID] ASC) WITH (FILLFACTOR = 90)
);

