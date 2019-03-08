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


CREATE NONCLUSTERED INDEX [IX_LeadScoreRules_missing_330] ON [dbo].[LeadScoreRules]
(
	[LeadScoreRuleMapID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
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