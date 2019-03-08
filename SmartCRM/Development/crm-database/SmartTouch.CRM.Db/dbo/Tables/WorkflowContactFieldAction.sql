CREATE TABLE [dbo].[WorkflowContactFieldAction] (
    [WorkflowContactFieldActionID] INT          IDENTITY (1, 1) NOT NULL,
    [WorkflowActionID]             INT          NOT NULL,
    [FieldID]                      INT          NULL,
    [FieldValue]                   NVARCHAR (MAX)  NULL,
    [DropdownValueID]              SMALLINT     NULL,
    CONSTRAINT [PK_WorkflowContactFieldAction] PRIMARY KEY CLUSTERED ([WorkflowContactFieldActionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_WorkflowContactFieldAction_DropdownValues] FOREIGN KEY ([DropdownValueID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_WorkflowContactFieldAction_Fields] FOREIGN KEY ([FieldID]) REFERENCES [dbo].[Fields] ([FieldID]),
    CONSTRAINT [FK_WorkflowContactFieldAction_WorkflowActions] FOREIGN KEY ([WorkflowActionID]) REFERENCES [dbo].[WorkflowActions] ([WorkflowActionID])
);

