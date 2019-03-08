CREATE TABLE [dbo].[OpportunityTagMap_Audit] (
    [AuditId]             BIGINT        IDENTITY (1, 1) NOT NULL,
    [OpportunityTagMapID] INT           NOT NULL,
    [OpportunityID]       INT           NOT NULL,
    [TagID]               INT           NOT NULL,
    [TaggedBy]            INT           NOT NULL,
    [TaggedOn]            DATETIME      NOT NULL,
    [AuditAction]         CHAR (1)      NOT NULL,
    [AuditDate]           DATETIME      CONSTRAINT [DF_OpportunityTagMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]           VARCHAR (50)  CONSTRAINT [DF_OpportunityTagMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]            VARCHAR (128) CONSTRAINT [DF_OpportunityTagMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]         BIT           NOT NULL,
    CONSTRAINT [PK_OpportunityTagMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90)
);

