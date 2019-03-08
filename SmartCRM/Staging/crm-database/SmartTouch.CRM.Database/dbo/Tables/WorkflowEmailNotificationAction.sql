CREATE TABLE [dbo].[WorkflowEmailNotificationAction] (
    [WorkFlowEmailNotificationActionID] INT           IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]                  INT           NOT NULL,
    [FromEmailID]                       INT           NOT NULL,
    [Subject]                           VARCHAR (100) NOT NULL,
    [Body]                              VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_WorkflowEmailNotificationAction] PRIMARY KEY CLUSTERED ([WorkFlowEmailNotificationActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowEmailNotificationAction_AccountEmails] FOREIGN KEY ([FromEmailID]) REFERENCES [dbo].[AccountEmails] ([EmailID]),
    CONSTRAINT [FK_WorkflowEmailNotificationAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

