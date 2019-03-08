CREATE TABLE [dbo].[WorkflowTagAction] (
    [WorkflowTagActionID] INT     IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]    INT     NOT NULL,
    [TagID]               INT     NOT NULL,
    [ActionType]          TINYINT NOT NULL,
    CONSTRAINT [PK_WorkflowTagAction] PRIMARY KEY CLUSTERED ([WorkflowTagActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowTagAction_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID]),
    CONSTRAINT [FK_WorkflowTagAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

