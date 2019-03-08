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
    [IncludeInReports]        BIT              DEFAULT ((1)) NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_missing_88]
    ON [dbo].[Contacts_Audit]([ContactID] ASC, [AccountID] ASC, [AuditAction] ASC)
    INCLUDE([LastUpdatedBy], [LastUpdatedOn]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_Audit_missing_15]
    ON [dbo].[Contacts_Audit]([ContactID] ASC, [AuditAction] ASC)
    INCLUDE([LastUpdatedBy], [LastUpdatedOn]) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-Contacts_Audit]
    ON [dbo].[Contacts_Audit]
    ON [AccountId_Scheme_UserActivityLogs] ([AccountID]);

