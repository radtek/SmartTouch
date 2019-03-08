CREATE TABLE [dbo].[WorkFlowUserAssignmentAction] (
    [WorkFlowUserAssignmentActionID] INT     IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]               INT     NOT NULL,
    [ScheduledID]                    TINYINT CONSTRAINT [Def_ScheduledID] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_WorkFlowUserAssignmentAction] PRIMARY KEY CLUSTERED ([WorkFlowUserAssignmentActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkFlowUserAssignmentAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

