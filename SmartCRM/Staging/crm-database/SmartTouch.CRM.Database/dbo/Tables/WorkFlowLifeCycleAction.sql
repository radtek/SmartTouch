CREATE TABLE [dbo].[WorkFlowLifeCycleAction] (
    [WorkFlowLifeCycleActionID] INT      IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]          INT      NOT NULL,
    [LifecycleDropdownValueID]  SMALLINT NOT NULL,
    CONSTRAINT [PK_WorkFlowLifeCycleAction] PRIMARY KEY CLUSTERED ([WorkFlowLifeCycleActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkFlowLifeCycleAction_DropdownValues] FOREIGN KEY ([LifecycleDropdownValueID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_WorkFlowLifeCycleAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

