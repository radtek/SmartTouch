CREATE TABLE [dbo].[TriggerWorkflowAction] (
    [TriggerWorkflowActionID] INT      IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]        INT      NOT NULL,
    [SiblingWorkflowID]       SMALLINT NOT NULL,
    CONSTRAINT [PK_TriggerWorkflowAction] PRIMARY KEY CLUSTERED ([TriggerWorkflowActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_TriggerWorkflowAction_TriggerWorkflowAction] FOREIGN KEY ([SiblingWorkflowID]) REFERENCES [dbo].[Workflows] ([WorkflowID]),
    CONSTRAINT [FK_TriggerWorkflowAction_TriggerWorkflowAction1] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

