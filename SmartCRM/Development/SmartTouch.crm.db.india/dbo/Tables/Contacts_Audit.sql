CREATE TABLE [dbo].[Contacts_Audit] (
    [AuditId]                 BIGINT           IDENTITY (1, 1) NOT NULL,
    [ContactID]               INT              NOT NULL,
    [FirstName]               NVARCHAR (150)   NULL,
    [LastName]                NVARCHAR (150)   NULL,
    [Company]                 NVARCHAR (150)   NULL,
    [CommunicationID]         INT              NULL,
    [Title]                   NVARCHAR (200)   NULL,
    [ContactImageUrl]         VARCHAR (2000)   NULL,
    [AccountID]               INT              NOT NULL,
    [LeadScore]               INT              NULL,
    [LeadSource]              NVARCHAR (200)   NULL,
    [HomePhone]               VARCHAR (20)     NULL,
    [WorkPhone]               VARCHAR (20)     NULL,
    [MobilePhone]             VARCHAR (20)     NULL,
    [PrimaryEmail]            VARCHAR (256)    NULL,
    [ContactType]             TINYINT          NOT NULL,
    [SSN]                     VARCHAR (200)    NULL,
    [LifecycleStage]          SMALLINT         NULL,
    [PartnerType]             SMALLINT         NULL,
    [DoNotEmail]              BIT              NULL,
    [LastContacted]           DATETIME         NULL,
    [IsDeleted]               BIT              NULL,
    [ProfileImageKey]         UNIQUEIDENTIFIER NULL,
    [ImageID]                 INT              NULL,
    [ReferenceID]             UNIQUEIDENTIFIER NULL,
    [LastUpdatedBy]           INT              NULL,
    [LastUpdatedOn]           DATETIME         NULL,
    [AuditAction]             CHAR (1)         NOT NULL,
    [AuditDate]               DATETIME         CONSTRAINT [DF_Contacts_Audit_AuditDate] DEFAULT (getutcdate()) NOT NULL,
    [AuditUser]               VARCHAR (50)     CONSTRAINT [DF_Contacts_Audit_AuditUser] DEFAULT (suser_sname()) NOT NULL,
    [AuditApp]                VARCHAR (128)    CONSTRAINT [DF_Contacts_Audit_AuditApp] DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') NULL,
    [AuditStatus]             BIT              NOT NULL,
    [ContactSource]           TINYINT          NULL,
    [SourceType]              INT              NULL,
    [CompanyID]               INT              NULL,
    [OwnerID]                 INT              NULL,
    [LastContactedThrough]    TINYINT          NULL,
    [IsLifecycleStageChanged] BIT              NULL,
    [NewLifecycleStage]       SMALLINT         NULL,
    [FirstContactSource]      TINYINT          NULL,
    [FirstSourceType]         INT              NULL,
    CONSTRAINT [PK_Contacts_Audit] PRIMARY KEY CLUSTERED ([AuditId] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY]
);


GO


CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_AuditAction_LastUpdatedOn]
    ON [dbo].[Contacts_Audit]([AuditAction] ASC, [LastUpdatedOn] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_ContactID]
    ON [dbo].[Contacts_Audit]([ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_ContactID_LastUpdatedBy]
    ON [dbo].[Contacts_Audit]([ContactID] ASC)
    INCLUDE([LastUpdatedBy], [LastUpdatedOn], [AuditAction]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_IsLifecycleStageChanged]
    ON [dbo].[Contacts_Audit]([IsLifecycleStageChanged] ASC)
    INCLUDE([ContactID], [LifecycleStage], [LastUpdatedBy], [AuditDate], [AuditStatus], [NewLifecycleStage]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO


CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_missing_1]
    ON [dbo].[Contacts_Audit]([AccountID] ASC, [ContactType] ASC, [IsDeleted] ASC, [AuditAction] ASC, [OwnerID] ASC)
    INCLUDE([ContactID], [AuditDate]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_missing_3]
    ON [dbo].[Contacts_Audit]([AccountID] ASC, [ContactType] ASC, [IsDeleted] ASC, [AuditAction] ASC, [OwnerID] ASC)
    INCLUDE([AuditDate]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO

CREATE NONCLUSTERED INDEX [ix_ContactsAudit_AccountIDAuditActionLastUpdatedBy]
    ON [dbo].[Contacts_Audit]([AccountID] ASC, [AuditAction] ASC, [LastUpdatedBy] ASC)
    INCLUDE([ContactID], [LastUpdatedOn]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];
GO

Create NonClustered Index IX_Contacts_Audit_missing_88 On [dbo].[Contacts_Audit] ([ContactID], [AccountID], [AuditAction]) Include ([LastUpdatedBy], [LastUpdatedOn]);
GO

Create NonClustered Index IX_Contacts_Audit_missing_15 On [dbo].[Contacts_Audit] ([ContactID], [AuditAction]) Include ([LastUpdatedBy], [LastUpdatedOn]);
GO


Create NonClustered Index IX_Contacts_Audit_missing_9 On [dbo].[Contacts_Audit] ([ContactID]);
GO