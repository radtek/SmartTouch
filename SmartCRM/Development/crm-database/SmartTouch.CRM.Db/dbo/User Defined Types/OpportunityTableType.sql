CREATE TYPE [dbo].[OpportunityTableType] AS TABLE(
	[OpportunityID] [int] NULL,
	[OpportunityName] [nvarchar](75) NOT NULL,
	[Potential] [money] NOT NULL,
	[StageID] [smallint] NOT NULL,
	[ExpectedClosingDate] [datetime] NULL,
	[Description] [nvarchar](1000) NULL,
	[Owner] [int] NOT NULL,
	[AccountID] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedOn] [datetime] NULL,
	[IsDeleted] [bit] NULL,
	[OpportunityType] [varchar](75) NULL,
	[ProductType] [varchar](75) NULL,
	[Address] [varchar](250) NULL,
	[ImageID] [int] NULL
)
GO