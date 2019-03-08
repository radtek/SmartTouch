CREATE TYPE [dbo].[OpportunityContactMapTableType] AS TABLE (
    [OpportunityContactMapID] INT            NULL,
    [OpportunityID]           INT            NOT NULL,
    [ContactID]               INT            NOT NULL,
    [Potential]               MONEY          NULL,
    [ExpectedToClose]         DATETIME       NULL,
    [Comments]                NVARCHAR (MAX) NULL,
    [Owner]                   INT            NULL,
    [StageID]                 SMALLINT       NULL,
    [IsDeleted]               BIT            NOT NULL,
    [CreatedOn]               DATETIME       NULL,
    [CreatedBy]               INT            NULL);

