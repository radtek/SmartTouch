

CREATE TABLE [dbo].[RoundRobinContactAssignment](
	[RoundRobinContactAssignmentID] [int] IDENTITY(1,1) NOT NULL,
	[DayOfWeek] [tinyint] NOT NULL,
	[IsRoundRobinAssignment] [bit] NOT NULL,
	[UserID] [varchar](100) NOT NULL,
	[WorkFlowUserAssignmentActionID] [int] NOT NULL,
	CONSTRAINT [Pk_RoundRobinContactAssignment] PRIMARY KEY CLUSTERED ([RoundRobinContactAssignmentID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_RoundRobinContactAssignment_WorkFlowUserAssignmentAction] FOREIGN KEY ([WorkFlowUserAssignmentActionID]) REFERENCES [dbo].[WorkFlowUserAssignmentAction] ([WorkFlowUserAssignmentActionID])
 )
GO




