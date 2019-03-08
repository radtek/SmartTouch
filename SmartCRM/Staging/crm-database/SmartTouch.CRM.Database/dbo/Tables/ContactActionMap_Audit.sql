CREATE TABLE [dbo].[ContactActionMap_Audit] (
    [AuditId]            BIGINT        IDENTITY (1, 1) NOT NULL,
    [ContactActionMapID] INT           NOT NULL,
    [ActionID]           INT           NOT NULL,
    [ContactID]          INT           NOT NULL,
    [IsCompleted]        BIT           NULL,
    [AuditAction]        CHAR (1)      NOT NULL,
    [AuditDate]          DATETIME      CONSTRAINT [DF_ContactActionMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]          VARCHAR (50)  CONSTRAINT [DF_ContactActionMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]           VARCHAR (128) CONSTRAINT [DF_ContactActionMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]        BIT           NOT NULL,
    [LastUpdatedBy]      INT           NULL,
    [LastUpdatedOn]      DATETIME      NULL,
    CONSTRAINT [PK_ContactActionMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactActionMap_Audit_ContactID_AuditAction]
    ON [dbo].[ContactActionMap_Audit]([ContactID] ASC, [AuditAction] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactActionMap_Audit_ContactActionMapID]
    ON [dbo].[ContactActionMap_Audit]([ContactActionMapID] ASC)
    INCLUDE([AuditId]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

