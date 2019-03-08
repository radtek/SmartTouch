CREATE TYPE [Workflow].[TrackActionType] AS TABLE (
    [TrackActionID]         BIGINT   NULL,
    [ExecutedOn]            DATETIME NULL,
    [ActionProcessStatusID] SMALLINT NULL);

