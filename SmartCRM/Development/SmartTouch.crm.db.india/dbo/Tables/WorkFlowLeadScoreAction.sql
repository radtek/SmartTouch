CREATE TABLE [dbo].[WorkFlowLeadScoreAction] (
    [WorkFlowLeadScoreActionID] INT IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]          INT NOT NULL,
    [LeadScoreValue]            INT NOT NULL,
    CONSTRAINT [PK_WorkFlowLeadScoreAction] PRIMARY KEY CLUSTERED ([WorkFlowLeadScoreActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkFlowLeadScoreAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

