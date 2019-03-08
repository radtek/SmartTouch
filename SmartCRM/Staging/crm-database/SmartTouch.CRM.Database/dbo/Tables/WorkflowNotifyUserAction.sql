CREATE TABLE [dbo].[WorkflowNotifyUserAction] (
    [WorkFlowNotifyUserActionID] INT           IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]           INT           NOT NULL,
    [UserID]                     VARCHAR (100) NOT NULL,
    [NotifyType]                 TINYINT       NOT NULL,
    [MessageBody]                VARCHAR (512) NULL,
    [NotificationFields]         VARCHAR (MAX) NULL,
    CONSTRAINT [PK_WorkflowNotifyUserAction] PRIMARY KEY CLUSTERED ([WorkFlowNotifyUserActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowNotifyUserAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowNotifyUserAction_missing_25]
    ON [dbo].[WorkflowNotifyUserAction]([WorkflowActionID] ASC)
    INCLUDE([WorkFlowNotifyUserActionID], [UserID], [NotifyType], [MessageBody], [NotificationFields]);

