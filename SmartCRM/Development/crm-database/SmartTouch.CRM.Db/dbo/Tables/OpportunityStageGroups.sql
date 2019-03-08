CREATE TABLE [dbo].[OpportunityStageGroups] (
    [OpportunityStageGroupID] INT      IDENTITY (1, 1) NOT NULL,
    [AccountID]               INT      NOT NULL,
    [DropdownValueID]         SMALLINT NOT NULL,
    [OpportunityGroupID]      INT      NOT NULL,
    CONSTRAINT [PK_OpportunityStageGroups] PRIMARY KEY CLUSTERED ([OpportunityStageGroupID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_OpportunityStageGroups_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_OpportunityStageGroups_DropdownValues] FOREIGN KEY ([DropdownValueID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_OpportunityStageGroups_OpportunityGroups] FOREIGN KEY ([OpportunityGroupID]) REFERENCES [dbo].[OpportunityGroups] ([OpportunityGroupID])
);

