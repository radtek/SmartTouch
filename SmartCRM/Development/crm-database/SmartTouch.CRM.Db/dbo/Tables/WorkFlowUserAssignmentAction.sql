
CREATE TABLE [dbo].[WorkFlowUserAssignmentAction](
	[WorkFlowUserAssignmentActionID] [int] IDENTITY(1,1) NOT NULL,
	[WorkflowActionID] [int] NOT NULL,
	[ScheduledID] [tinyint] NOT NULL CONSTRAINT [Def_ScheduledID]  DEFAULT ((1)),
CONSTRAINT [PK_WorkFlowUserAssignmentAction] PRIMARY KEY CLUSTERED ([WorkFlowUserAssignmentActionID] ASC) WITH (FILLFACTOR = 90),
CONSTRAINT [FK_WorkFlowUserAssignmentAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);
GO






