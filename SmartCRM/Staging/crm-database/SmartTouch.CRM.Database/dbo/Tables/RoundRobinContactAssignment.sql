CREATE TABLE [dbo].[RoundRobinContactAssignment] (
    [RoundRobinContactAssignmentID]  INT           IDENTITY (1, 1) NOT NULL,
    [DayOfWeek]                      TINYINT       NOT NULL,
    [IsRoundRobinAssignment]         BIT           NOT NULL,
    [UserID]                         VARCHAR (100) NOT NULL,
    [WorkFlowUserAssignmentActionID] INT           NOT NULL,
    CONSTRAINT [Pk_RoundRobinContactAssignment] PRIMARY KEY CLUSTERED ([RoundRobinContactAssignmentID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkFlowUserAssignmentAction] FOREIGN KEY ([WorkFlowUserAssignmentActionID]) REFERENCES [dbo].[WorkFlowUserAssignmentAction] ([WorkFlowUserAssignmentActionID])
);

