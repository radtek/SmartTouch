CREATE TABLE [dbo].[WorkflowActions] (
    [WorkflowActionID]     INT      IDENTITY (1, 1) NOT NULL,
    [WorkflowActionTypeID] TINYINT  NOT NULL,
    [WorkflowID]           SMALLINT NOT NULL,
    [OrderNumber]          INT      NOT NULL,
    [IsDeleted]            BIT      NOT NULL,
    [IsSubAction]          BIT      NOT NULL,
    CONSTRAINT [PK_WorkflowActions] PRIMARY KEY CLUSTERED ([WorkflowActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowActions_WorkflowActionTypes] FOREIGN KEY ([WorkflowActionTypeID]) REFERENCES [dbo].[WorkflowActionTypes] ([WorkflowActionTypeID]),
    CONSTRAINT [FK_WorkflowActions_Workflows] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflows] ([WorkflowID])
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowActions_WorkflowID]
    ON [dbo].[WorkflowActions]([WorkflowID] ASC)
    INCLUDE([WorkflowActionID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowActions_WorkflowID_IsSubAction]
    ON [dbo].[WorkflowActions]([WorkflowID] ASC, [IsSubAction] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowActions_WorkflowActionTypeID]
    ON [dbo].[WorkflowActions]([WorkflowActionTypeID] ASC, [WorkflowID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowActions_WorkflowID_IsDeleted_IsSubAction_WorkflowActionTypeID]
    ON [dbo].[WorkflowActions]([WorkflowID] ASC, [IsDeleted] ASC, [IsSubAction] ASC, [WorkflowActionTypeID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowActions_WorkflowID_IsDeleted]
    ON [dbo].[WorkflowActions]([WorkflowID] ASC, [IsDeleted] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowActions_WorkflowID_IsDeleted_WorkflowActionTypeID]
    ON [dbo].[WorkflowActions]([WorkflowID] ASC, [IsDeleted] ASC, [WorkflowActionTypeID] ASC) WITH (FILLFACTOR = 90);

