

CREATE TABLE [dbo].[RoundRobinContactAssignment](
	[RoundRobinContactAssignmentID] [int] IDENTITY(1,1) NOT NULL,
	[DayOfWeek] [tinyint] NOT NULL,
	[IsRoundRobinAssignment] [bit] NOT NULL,
	[UserID] [varchar](100) NOT NULL,
	[WorkFlowUserAssignmentActionID] [int] NOT NULL,
 CONSTRAINT [Pk_RoundRobinContactAssignment] PRIMARY KEY CLUSTERED 
(
	[RoundRobinContactAssignmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



ALTER TABLE [dbo].[RoundRobinContactAssignment]  WITH CHECK ADD  CONSTRAINT [FK_WorkFlowUserAssignmentAction] FOREIGN KEY([WorkFlowUserAssignmentActionID])
REFERENCES [dbo].[WorkFlowUserAssignmentAction] ([WorkFlowUserAssignmentActionID])
GO

ALTER TABLE [dbo].[RoundRobinContactAssignment] CHECK CONSTRAINT [FK_WorkFlowUserAssignmentAction]
GO


