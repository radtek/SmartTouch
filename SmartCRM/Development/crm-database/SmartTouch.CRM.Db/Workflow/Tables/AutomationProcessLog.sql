
CREATE TABLE [Workflow].[AutomationProcessLog](
	[AutomationProcessLogID] [int] IDENTITY(1,1) NOT NULL,
	[WorkflowId] [int] NULL,
	[TrackMessageId] [bigint] NULL,
	[Remarks] [varchar](max) NULL,
	[CreatedOn] [datetime] NULL DEFAULT (getutcdate())
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


