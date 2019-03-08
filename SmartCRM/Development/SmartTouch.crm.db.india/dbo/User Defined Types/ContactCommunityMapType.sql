
CREATE TYPE [dbo].[ContactCommunityMapType] AS TABLE(
	[ContactCommunityMapID] [int] NOT NULL,
	[ContactId] [int] NOT NULL,
	[CommunityID] [smallint] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
	[IsDeleted] [bit] NOT NULL
)
GO
