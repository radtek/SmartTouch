CREATE TABLE [dbo].[EndTriggerMatchAudit] (
    [EndTriggerMatchAuditID] INT     IDENTITY (1, 1) NOT NULL,
    [ContactID]              INT     NULL,
    [WorkflowID]             INT     NULL,
    [TrackMessageID]         INT     NULL,
    [LeadScoreConditionType] TINYINT NULL,
    [ContactValue]           INT     NULL,
    [WorkflowValue]          INT     NULL,
    [HasMatched]             BIT     NOT NULL,
    PRIMARY KEY CLUSTERED ([EndTriggerMatchAuditID] ASC)
);

