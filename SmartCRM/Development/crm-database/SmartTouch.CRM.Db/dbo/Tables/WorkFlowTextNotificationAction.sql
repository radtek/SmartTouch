CREATE TABLE [dbo].[WorkFlowTextNotificationAction] (
    [WorkFlowTextNotificationActionID] INT           IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]                 INT           NOT NULL,
    [FromMobileID]                     INT           NOT NULL,
    [Message]                          VARCHAR (160) NULL,
    CONSTRAINT [PK_WorkFlowTextNotificationAction] PRIMARY KEY CLUSTERED ([WorkFlowTextNotificationActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkFlowTextNotificationAction_ContactPhoneNumbers] FOREIGN KEY ([FromMobileID]) REFERENCES [dbo].[ContactPhoneNumbers] ([ContactPhoneNumberID]),
    CONSTRAINT [FK_WorkFlowTextNotificationAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

