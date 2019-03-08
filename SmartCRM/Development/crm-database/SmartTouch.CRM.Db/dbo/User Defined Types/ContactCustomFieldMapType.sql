
CREATE TYPE [dbo].[ContactCustomFieldMapType] AS TABLE(
	[ContactCustomFieldMapId] [int] NULL,
	[ContactId] [int] NULL,
	[CustomFieldId] [int] NULL,
	[Value] [nvarchar](MAX) NULL
)
GO


