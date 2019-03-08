
CREATE TYPE [dbo].[ContactEmailType] AS TABLE(
	[ContactEmailID] [int] NULL,
	[ContactID] [int] NULL,
	[EmailId] [varchar](256) NULL,
	[EmailStatus] [tinyint] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
	[AccountID] [int] NOT NULL,
	[SnoozeUntil] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL
)
GO


