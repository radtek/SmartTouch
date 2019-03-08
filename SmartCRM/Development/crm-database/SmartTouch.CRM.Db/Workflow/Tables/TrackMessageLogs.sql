CREATE TABLE [Workflow].[TrackMessageLogs] (
    [TrackMessageLogID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [TrackMessageID]    BIGINT         NOT NULL,
    [ErrorMessage]      NVARCHAR(MAX)  NULL,
    [CreatedOn]         DATETIME       DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_TrackMessageLogs] PRIMARY KEY CLUSTERED ([TrackMessageLogID] ASC) WITH (FILLFACTOR = 90) ON [WorkflowFileGroup]
);

