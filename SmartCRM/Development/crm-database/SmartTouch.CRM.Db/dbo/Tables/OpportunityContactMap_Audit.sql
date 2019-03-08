CREATE TABLE [dbo].[OpportunityContactMap_Audit] (
    [AuditId]                 BIGINT        IDENTITY (1, 1) NOT NULL,
    [OpportunityContactMapID] INT           NOT NULL,
    [OpportunityID]           INT           NOT NULL,
    [ContactID]               INT           NOT NULL,
    [AuditAction]             CHAR (1)      NOT NULL,
    [AuditDate]               DATETIME      CONSTRAINT [DF_OpportunityContactMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]               VARCHAR (50)  CONSTRAINT [DF_OpportunityContactMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]                VARCHAR (128) CONSTRAINT [DF_OpportunityContactMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]             BIT           NOT NULL,
	[Potential]               MONEY         NULL,
	[ExpectedToClose]         DATETIME      NULL, 
	[Comments]                NVARCHAR(MAX) NULL,
	[Owner]                   INT            NULL,
	[StageID]                 SMALLINT       NULL,
	[IsDeleted]               BIT DEFAULT(0) NOT NULL,
	CreatedOn                 DATETIME NULL,
	CreatedBy                 INT NULL
    CONSTRAINT [PK_OpportunityContactMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90)
);

