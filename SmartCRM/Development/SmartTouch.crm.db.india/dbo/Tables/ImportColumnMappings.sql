
CREATE TABLE [dbo].[ImportColumnMappings](
	[ImportColumnMappingID] [int] IDENTITY(1,1) NOT NULL,
	[SheetColumnName] [varchar](125) NULL,
	[IsCustomField] [bit] NULL,
	[IsDropDownField] [bit] NULL,
	[ContactFieldName] [varchar](75) NULL,
	[JobID] [int] NOT NULL,
CONSTRAINT [PK_ImportColumnMappings] PRIMARY KEY CLUSTERED (ImportColumnMappingID ASC) WITH (FILLFACTOR = 90));
    
GO





