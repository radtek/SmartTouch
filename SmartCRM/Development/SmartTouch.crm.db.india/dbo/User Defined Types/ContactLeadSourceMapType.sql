
CREATE TYPE [dbo].[ContactLeadSourceMapType] AS TABLE(
	[ContactLeadSourceMapID] [int] NULL,
	[ContactId] [int] NOT NULL,
	[LeadSouceID] [smallint] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL
)
GO


