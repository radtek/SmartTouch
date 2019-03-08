CREATE TABLE [dbo].[ContactTourMap_Audit] (
    [AuditId]          BIGINT        IDENTITY (1, 1) NOT NULL,
    [ContactTourMapID] INT           NOT NULL,
    [TourID]           INT           NOT NULL,
    [ContactID]        INT           NOT NULL,
    [AuditAction]      CHAR (1)      NOT NULL,
    [AuditDate]        DATETIME      CONSTRAINT [DF_ContactTourMap_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]        VARCHAR (50)  CONSTRAINT [DF_ContactTourMap_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]         VARCHAR (128) CONSTRAINT [DF_ContactTourMap_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]      BIT           NOT NULL,
    [IsCompleted]      BIT           NOT NULL,
    [LastUpdatedBy]    INT           NULL,
    [LastUpdatedOn]    DATETIME      NULL,
    CONSTRAINT [PK_ContactTourMap_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTourMap_Audit_ContactID_AuditAction]
    ON [dbo].[ContactTourMap_Audit]([ContactID] ASC, [AuditAction] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactTourMap_Audit_ContactTourMapID]
    ON [dbo].[ContactTourMap_Audit]([ContactTourMapID] ASC)
    INCLUDE([AuditId]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

