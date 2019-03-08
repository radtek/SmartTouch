CREATE TABLE [dbo].[LeadScores] (
    [LeadScoreID]     INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]       INT      NOT NULL,
    [LeadScoreRuleID] INT      NOT NULL,
    [Score]           INT      NULL,
    [AddedOn]         DATETIME NOT NULL,
    [EntityID]        INT      NOT NULL,
    PRIMARY KEY CLUSTERED ([LeadScoreID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadScores_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_LeadScores_LeadScoreRules] FOREIGN KEY ([LeadScoreRuleID]) REFERENCES [dbo].[LeadScoreRules] ([LeadScoreRuleID])
);


GO

CREATE NONCLUSTERED INDEX [IX_LeadScores_ContactID]
    ON [dbo].[LeadScores]([ContactID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_LeadScores_ContactID_EntityID]
    ON [dbo].[LeadScores]([ContactID] ASC, [EntityID] ASC) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_LeadScores_LeadScoreRuleID]
    ON [dbo].[LeadScores]([LeadScoreRuleID] ASC)
    INCLUDE([LeadScoreID], [ContactID], [Score], [AddedOn], [EntityID]) WITH (FILLFACTOR = 90);
GO

Create NonClustered Index IX_LeadScoreRules_AccountID On [dbo].[LeadScoreRules] ([AccountID], [IsActive],[ConditionID], [ConditionValue]);
GO

Create NonClustered Index IX_LeadScoreRules_AccountID_ConditionValue On [dbo].[LeadScoreRules] ([AccountID], [ConditionValue],[ConditionID]) Include ([LeadScoreRuleID]);
GO

Create NonClustered Index IX_LeadScoreRules_ConditionID On [dbo].[LeadScoreRules] ([ConditionID]) Include ([LeadScoreRuleID], [AccountID], [ConditionValue]);
GO

Create NonClustered Index IX_LeadScoreRules_AccountID_IsActive On [dbo].[LeadScoreRules] ([AccountID], [IsActive],[ConditionID]);
GO

Create NonClustered Index IX_LeadScoreRules_ConditionValue On [dbo].[LeadScoreRules] ([ConditionValue], [IsActive],[ConditionID]);
GO