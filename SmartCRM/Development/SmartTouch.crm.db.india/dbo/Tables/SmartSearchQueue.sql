
CREATE TABLE [dbo].[SmartSearchQueue](
	[ResultID] [int] IDENTITY(1,1) NOT NULL,
	[SearchDefinitionID] [int] NOT NULL,
	[IsProcessed] [bit] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[AccountID] [int] NOT NULL
) ON [PRIMARY]

GO


