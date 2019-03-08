CREATE TYPE [dbo].[OpportunityContactMapTableType] AS TABLE(
	[OpportunityContactMapID] [int] NULL,
	[OpportunityID] [int] NOT NULL,
	[ContactID] [int] NOT NULL,
	[Potential] [money] NULL,
	[ExpectedToClose] [datetime] NULL,
	[Comments] [nvarchar](max) NULL,
	[Owner] [int] NULL,
	[StageID] [smallint] NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [int] NULL
)
GO