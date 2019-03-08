
CREATE TYPE [dbo].[ImageType] AS TABLE(
	[ImageID] [int] NULL,
	[FriendlyName] [nvarchar](2000) NULL,
	[StorageName] [varchar](2000) NULL,
	[OriginalName] [nvarchar](2000) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[CategoryId] [tinyint] NULL,
	[AccountID] [int] NULL
)
GO


