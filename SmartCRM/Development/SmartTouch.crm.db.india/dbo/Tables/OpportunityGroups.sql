CREATE TABLE [dbo].[OpportunityGroups] (
    [OpportunityGroupID]   INT          IDENTITY (1, 1) NOT NULL,
    [OpportunityGroupName] VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_OpportunityGroups] PRIMARY KEY CLUSTERED ([OpportunityGroupID] ASC) WITH (FILLFACTOR = 90)
);

