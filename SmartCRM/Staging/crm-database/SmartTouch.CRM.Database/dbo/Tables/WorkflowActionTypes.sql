CREATE TABLE [dbo].[WorkflowActionTypes] (
    [WorkflowActionTypeID] TINYINT      IDENTITY (1, 1) NOT NULL,
    [WorkflowActionName]   VARCHAR (75) NOT NULL,
    [IsLinkAction]         BIT          NOT NULL,
    CONSTRAINT [PK_WorkflowActionTypes] PRIMARY KEY CLUSTERED ([WorkflowActionTypeID] ASC) WITH (FILLFACTOR = 90)
);

