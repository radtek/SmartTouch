CREATE TABLE [Workflow].[AutomationProcessLog] (
    [AutomationProcessLogID] INT           IDENTITY (1, 1) NOT NULL,
    [WorkflowId]             INT           NULL,
    [TrackMessageId]         BIGINT        NULL,
    [Remarks]                VARCHAR (MAX) NULL,
    [CreatedOn]              DATETIME      DEFAULT (getutcdate()) NULL
);

