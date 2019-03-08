CREATE TABLE [dbo].[ContactWorkflowAudit] (
    [ContactWorkflowAuditID] INT          IDENTITY (1, 1) NOT NULL,
    [ContactID]              INT          NOT NULL,
    [WorkflowID]             SMALLINT     NOT NULL,
    [WorkflowActionID]       INT          NOT NULL,
    [ActionPerformedOn]      DATETIME     NOT NULL,
    [MessageID]              VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_ContactWorkflowAudit] PRIMARY KEY CLUSTERED ([ContactWorkflowAuditID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_ContactWorkflowAudit_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactWorkflowAudit_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID]),
    CONSTRAINT [FK_ContactWorkflowAudit_Workflows] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflows] ([WorkflowID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWorkflowAudit_ContactID_WorkflowID_WorkflowActionID]
    ON [dbo].[ContactWorkflowAudit]([ContactID] ASC, [WorkflowID] ASC, [WorkflowActionID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactWorkflowAudit_WorkflowID_WorkflowActionID]
    ON [dbo].[ContactWorkflowAudit]([WorkflowID] ASC, [WorkflowActionID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

