CREATE TABLE [dbo].[WorkflowUserAssignmentAudit] (
    [WorkflowUserAssignmentAuditID]  INT     IDENTITY (1, 1) NOT NULL,
    [ContactID]                      INT     NOT NULL,
    [UserID]                         INT     NOT NULL,
    [WorkflowUserAssignmentActionID] INT     NOT NULL,
    [DayOfWeek]                      TINYINT NOT NULL,
    CONSTRAINT [PK_WorkflowUserAssignmentAudit] PRIMARY KEY CLUSTERED ([WorkflowUserAssignmentAuditID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowUserAssignmentAudit_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_WorkflowUserAssignmentAudit_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_WorkflowUserAssignmentAudit_WorkFlowUserAssignmentAction] FOREIGN KEY ([WorkflowUserAssignmentActionID]) REFERENCES [dbo].[WorkFlowUserAssignmentAction] ([WorkFlowUserAssignmentActionID])
);


GO
CREATE NONCLUSTERED INDEX [IX_WorkflowUserAssignmentAudit_missing_133]
    ON [dbo].[WorkflowUserAssignmentAudit]([ContactID] ASC, [WorkflowUserAssignmentActionID] ASC);

