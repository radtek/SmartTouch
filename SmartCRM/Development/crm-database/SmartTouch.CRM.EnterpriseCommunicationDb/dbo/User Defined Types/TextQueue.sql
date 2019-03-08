
CREATE TYPE [dbo].[TextQueue] AS TABLE(
	[TextResponseID] [int] NULL,
	[From] [varchar](75) NULL,
	[To] [varchar](75) NULL,
	[SenderID] [varchar](75) NULL,
	[Message] [varchar](200) NULL,
	[Status] [bit] NULL,
	[ServiceResponse] [varchar](max) NULL,
	[RequestGuid] [uniqueidentifier] NULL,
	[TokenGuid] [uniqueidentifier] NULL,
	[ScheduledTime] [datetime] NULL
)
GO


