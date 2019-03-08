CREATE TABLE [dbo].[LeadScoreRules] (
    [LeadScoreRuleID]          INT            IDENTITY (1, 1) NOT NULL,
    [AccountID]                INT            NOT NULL,
    [ConditionDescription]     NVARCHAR (500) NULL,
    [ConditionID]              TINYINT        NOT NULL,
    [Score]                    INT            NOT NULL,
    [ConditionValue]           NVARCHAR (200) NULL,
    [AppliedToPreviousActions] BIT            NULL,
    [IsActive]                 BIT            NULL,
    [CreatedBy]                INT            NOT NULL,
    [CreatedOn]                DATETIME       NOT NULL,
    [ModifiedBy]               INT            NOT NULL,
    [ModifiedOn]               DATETIME       NOT NULL,
    [LeadScoreRuleMapID]       INT            NOT NULL,
    [SelectedCampaignLinks]    VARCHAR (MAX)  NULL,
    CONSTRAINT [PK_LeadScoreRules] PRIMARY KEY CLUSTERED ([LeadScoreRuleID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadScoreRules_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_LeadScoreRules_Conditions] FOREIGN KEY ([ConditionID]) REFERENCES [dbo].[Conditions] ([ConditionID]),
    CONSTRAINT [FK_LeadScoreRules_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_LeadScoreRules_Users1] FOREIGN KEY ([ModifiedBy]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_AccountID]
    ON [dbo].[LeadScoreRules]([AccountID] ASC, [IsActive] ASC, [ConditionID] ASC, [ConditionValue] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_AccountID_ConditionValue]
    ON [dbo].[LeadScoreRules]([AccountID] ASC, [ConditionValue] ASC, [ConditionID] ASC)
    INCLUDE([LeadScoreRuleID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_ConditionID]
    ON [dbo].[LeadScoreRules]([ConditionID] ASC)
    INCLUDE([LeadScoreRuleID], [AccountID], [ConditionValue]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_AccountID_IsActive]
    ON [dbo].[LeadScoreRules]([AccountID] ASC, [IsActive] ASC, [ConditionID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_ConditionValue]
    ON [dbo].[LeadScoreRules]([ConditionValue] ASC, [IsActive] ASC, [ConditionID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_missing_330]
    ON [dbo].[LeadScoreRules]([LeadScoreRuleMapID] ASC);

