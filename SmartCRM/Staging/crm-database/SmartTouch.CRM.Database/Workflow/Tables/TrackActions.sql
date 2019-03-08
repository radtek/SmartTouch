CREATE TABLE [Workflow].[TrackActions] (
    [TrackActionID]         BIGINT   IDENTITY (1, 1) NOT NULL,
    [TrackMessageID]        BIGINT   NOT NULL,
    [WorkflowID]            INT      NOT NULL,
    [ActionID]              INT      NOT NULL,
    [ScheduledOn]           DATETIME NOT NULL,
    [ExecutedOn]            DATETIME NULL,
    [CreatedOn]             DATETIME NOT NULL,
    [ActionProcessStatusID] SMALLINT NOT NULL,
    [WorkflowActionTypeID]  TINYINT  DEFAULT ((0)) NULL,
    CONSTRAINT [PK_TrackActions_TrackActionID] PRIMARY KEY CLUSTERED ([TrackActionID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON) ON [WorkflowFileGroup]
);


GO
CREATE NONCLUSTERED INDEX [IX_TrackActions_ActionProcessStatusID_ScheduledOn]
    ON [Workflow].[TrackActions]([ActionProcessStatusID] ASC, [ScheduledOn] ASC)
    INCLUDE([TrackActionID], [TrackMessageID], [WorkflowID], [ActionID], [ExecutedOn], [CreatedOn], [WorkflowActionTypeID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];


GO
CREATE NONCLUSTERED INDEX [IX_TrackActions_TrackMessageID_ActionProcessStatusID]
    ON [Workflow].[TrackActions]([TrackMessageID] ASC, [ActionProcessStatusID] ASC)
    INCLUDE([TrackActionID], [WorkflowID], [ActionID], [ScheduledOn], [ExecutedOn], [CreatedOn], [WorkflowActionTypeID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];


GO
CREATE NONCLUSTERED INDEX [IX_TrackActions_TrackMessageID_WorkflowActionTypeID]
    ON [Workflow].[TrackActions]([TrackMessageID] ASC, [WorkflowActionTypeID] ASC)
    INCLUDE([TrackActionID], [WorkflowID], [ActionID], [ScheduledOn], [ExecutedOn], [CreatedOn], [ActionProcessStatusID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];


GO
CREATE NONCLUSTERED INDEX [IX_TrackActions_WorkflowID_ActionProcessStatusID]
    ON [Workflow].[TrackActions]([WorkflowID] ASC, [ActionProcessStatusID] ASC)
    INCLUDE([TrackActionID], [TrackMessageID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];

