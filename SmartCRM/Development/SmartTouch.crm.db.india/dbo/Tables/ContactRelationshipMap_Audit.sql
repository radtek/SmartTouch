CREATE TABLE [dbo].[ContactRelationshipMap_Audit] (
    [AuditId]                  BIGINT        IDENTITY (1, 1) NOT NULL,
    [ContactRelationshipMapID] INT           NOT NULL,
    [ContactID]                INT           NOT NULL,
    [RelationshipType]         SMALLINT      NOT NULL,
    [RelatedUserID]            INT           NULL,
    [RelatedContactID]         INT           NULL,
    [AuditAction]              CHAR (1)      NOT NULL,
    [AuditDate]                DATETIME      CONSTRAINT [DF_ContactRelationshipMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]                VARCHAR (50)  CONSTRAINT [DF_ContactRelationshipMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]                 VARCHAR (128) CONSTRAINT [DF_ContactRelationshipMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]              BIT           NOT NULL,
    [CreatedBy]                INT           NOT NULL,
    [CreatedOn]                DATETIME      NOT NULL,
    CONSTRAINT [PK_ContactRelationshipMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90)
);

