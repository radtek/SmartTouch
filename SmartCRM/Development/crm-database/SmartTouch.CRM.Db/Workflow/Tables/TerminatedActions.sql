CREATE TABLE [Workflow].[TerminatedActions] (
    [TerminatedActionID]        BIGINT IDENTITY (1, 1) NOT NULL,
    [TrackActionID]             BIGINT NULL,
    [TerminatedActionMessageID] BIGINT NULL,
    CONSTRAINT [PK_TerminatedActions_TerminatedActionID] PRIMARY KEY CLUSTERED ([TerminatedActionID] ASC) WITH (FILLFACTOR = 90) ON [WorkflowFileGroup],
    CONSTRAINT [FK_TerminatedActions_TrackActionID] FOREIGN KEY ([TrackActionID]) REFERENCES [Workflow].[TrackActions] ([TrackActionID])
);

