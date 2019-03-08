CREATE TABLE [dbo].[EmailNotificationSettings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](100) NOT NULL,
	[Key] [varchar](100) NOT NULL,
	[CategoryID] [smallint] NOT NULL,
	CONSTRAINT [PK_WorkflowNotificationSettings] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (FILLFACTOR = 90)
)
GO