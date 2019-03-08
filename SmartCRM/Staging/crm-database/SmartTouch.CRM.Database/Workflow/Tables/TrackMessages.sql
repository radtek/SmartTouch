CREATE TABLE [Workflow].[TrackMessages] (
    [TrackMessageID]         BIGINT           IDENTITY (1, 1) NOT NULL,
    [MessageID]              UNIQUEIDENTIFIER NULL,
    [LeadScoreConditionType] TINYINT          NULL,
    [EntityID]               INT              NULL,
    [LinkedEntityID]         INT              CONSTRAINT [DF_TrackMessages_LinkedEntityID] DEFAULT ((0)) NULL,
    [ContactID]              INT              NULL,
    [UserID]                 INT              CONSTRAINT [DF_TrackMessages_UserID] DEFAULT ((0)) NULL,
    [AccountID]              INT              NULL,
    [ConditionValue]         VARCHAR (5000)   NULL,
    [CreatedOn]              DATETIME         CONSTRAINT [DF_TrackMessages_CreatedOn_1] DEFAULT (getutcdate()) NULL,
    [MessageProcessStatusID] SMALLINT         CONSTRAINT [DF_TrackMessages_MessageProcessStatusID] DEFAULT ((701)) NOT NULL,
    CONSTRAINT [PK_TrackMessages] PRIMARY KEY CLUSTERED ([TrackMessageID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON) ON [WorkflowFileGroup],
    CONSTRAINT [FK_TrackMessages_AccountId] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_TrackMessages_MessageProcessStatusID] FOREIGN KEY ([MessageProcessStatusID]) REFERENCES [dbo].[Statuses] ([StatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TrackMessages_ContactID]
    ON [Workflow].[TrackMessages]([ContactID] ASC)
    INCLUDE([TrackMessageID], [MessageID], [LeadScoreConditionType], [EntityID], [LinkedEntityID], [UserID], [AccountID], [ConditionValue], [CreatedOn], [MessageProcessStatusID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];


GO
CREATE NONCLUSTERED INDEX [IX_TrackMessages_missing_176]
    ON [Workflow].[TrackMessages]([LeadScoreConditionType] ASC, [MessageProcessStatusID] ASC)
    INCLUDE([TrackMessageID], [EntityID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];


GO
CREATE NONCLUSTERED INDEX [IX_TrackMessages_MessageProcessStatusID]
    ON [Workflow].[TrackMessages]([MessageProcessStatusID] ASC)
    INCLUDE([LeadScoreConditionType], [AccountID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [WorkflowFileGroup];

