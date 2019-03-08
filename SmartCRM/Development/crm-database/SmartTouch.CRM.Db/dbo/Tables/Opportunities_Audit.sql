CREATE TABLE [dbo].[Opportunities_Audit] (
    [AuditId]             BIGINT          IDENTITY (1, 1) NOT NULL,
    [OpportunityID]       INT             NOT NULL,
    [OpportunityName]     NVARCHAR (150)  NOT NULL,
    [Potential]           MONEY           NOT NULL,
    [StageID]             SMALLINT        NOT NULL,
    [ExpectedClosingDate] DATETIME        NULL,
    [Description]         NVARCHAR (2000) NULL,
    [Owner]               INT             NOT NULL,
    [AccountID]           INT             NOT NULL,
    [CreatedBy]           INT             NOT NULL,
    [CreatedOn]           DATETIME        NOT NULL,
    [LastModifiedBy]      INT             NULL,
    [LastModifiedOn]      DATETIME        NULL,
    [IsDeleted]           BIT             NULL,
    [AuditAction]         CHAR (1)        NOT NULL,
    [AuditDate]           DATETIME        CONSTRAINT [DF_Opportunities_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]           VARCHAR (50)    CONSTRAINT [DF_Opportunities_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]            VARCHAR (128)   CONSTRAINT [DF_Opportunities_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]         BIT             NOT NULL,
	OpportunityType       VARCHAR(75)     NULL,
	ProductType           VARCHAR(75)     NULL,
	[Address]             VARCHAR(250)    NULL, 
	ImageID               INT             NULL
    CONSTRAINT [PK_Opportunities_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90)
);

