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

CREATE NONCLUSTERED INDEX [Idx_WorkflowActions_work]
    ON [dbo].[WorkflowActions]([WorkflowActionTypeID] ASC, [IsSubAction] ASC, [IsDeleted] ASC, [WorkflowID] ASC)
    INCLUDE([OrderNumber]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowID On [dbo].[WorkflowActions] ([WorkflowID]) Include ([WorkflowActionID]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowID_IsDeleted_IsSubAction On [dbo].[WorkflowActions] ([WorkflowID], [IsDeleted], [IsSubAction]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowID_IsSubAction On [dbo].[WorkflowActions] ([WorkflowID], [IsSubAction]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowActionTypeID On [dbo].[WorkflowActions] ([WorkflowActionTypeID], [WorkflowID],[IsDeleted]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowID_IsDeleted_IsSubAction_WorkflowActionTypeID On [dbo].[WorkflowActions] ([WorkflowID], [IsDeleted], [IsSubAction],[WorkflowActionTypeID]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowID_IsDeleted On [dbo].[WorkflowActions] ([WorkflowID], [IsDeleted]);
GO

Create NonClustered Index IX_WorkflowActions_WorkflowID_IsDeleted_WorkflowActionTypeID On [dbo].[WorkflowActions] ([WorkflowID], [IsDeleted],[WorkflowActionTypeID]);
GO