CREATE TYPE [dbo].[OpportunityTableType] AS TABLE (
    [OpportunityID]       INT             NULL,
    [OpportunityName]     NVARCHAR (75)   NOT NULL,
    [Potential]           MONEY           NOT NULL,
    [StageID]             SMALLINT        NOT NULL,
    [ExpectedClosingDate] DATETIME        NULL,
    [Description]         NVARCHAR (1000) NULL,
    [Owner]               INT             NOT NULL,
    [AccountID]           INT             NOT NULL,
    [CreatedBy]           INT             NOT NULL,
    [CreatedOn]           DATETIME        NOT NULL,
    [LastModifiedBy]      INT             NULL,
    [LastModifiedOn]      DATETIME        NULL,
    [IsDeleted]           BIT             NULL,
    [OpportunityType]     VARCHAR (75)    NULL,
    [ProductType]         VARCHAR (75)    NULL,
    [Address]             VARCHAR (250)   NULL,
    [ImageID]             INT             NULL);

