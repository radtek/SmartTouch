

CREATE TABLE [dbo].[WorkflowUserAssignmentAudit](
	[WorkflowUserAssignmentAuditID] [int] IDENTITY(1,1) NOT NULL,
	[ContactID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[WorkflowUserAssignmentActionID] [int] NOT NULL,
	[DayOfWeek] [tinyint] NOT NULL,
CONSTRAINT [PK_WorkflowUserAssignmentAudit] PRIMARY KEY CLUSTERED ([WorkflowUserAssignmentAuditID] ASC) WITH (FILLFACTOR = 90),
CONSTRAINT [FK_WorkflowUserAssignmentAudit_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
CONSTRAINT [FK_WorkflowUserAssignmentAudit_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
CONSTRAINT [FK_WorkflowUserAssignmentAudit_WorkFlowUserAssignmentAction] FOREIGN KEY ([WorkflowUserAssignmentActionID]) REFERENCES [dbo].[WorkFlowUserAssignmentAction] ([WorkflowUserAssignmentActionID])
);
GO


Create NonClustered Index IX_WorkflowUserAssignmentAudit_missing_133 On [dbo].[WorkflowUserAssignmentAudit] ([ContactID], [WorkflowUserAssignmentActionID]);
GO




