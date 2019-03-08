CREATE TABLE [Workflow].[TrackActionLogs] (
    [TrackActionLogID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [TrackActionID]    BIGINT         NULL,
    [ErrorMessage]     NVARCHAR(MAX)  NULL ,
    [CreatedOn]        DATETIME       DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_TrackActionLogs_TerminatedActionID] PRIMARY KEY CLUSTERED ([TrackActionLogID] ASC) WITH (FILLFACTOR = 90) ON [WorkflowFileGroup],
    CONSTRAINT [FK_TrackActionLogs_TrackActionID] FOREIGN KEY ([TrackActionID]) REFERENCES [Workflow].[TrackActions] ([TrackActionID])
);

