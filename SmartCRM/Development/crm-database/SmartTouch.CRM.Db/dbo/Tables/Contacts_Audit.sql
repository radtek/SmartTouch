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
    
);


GO

Create NonClustered Index IX_Contacts_Audit_missing_88 On [dbo].[Contacts_Audit] ([ContactID], [AccountID], [AuditAction]) Include ([LastUpdatedBy], [LastUpdatedOn]);
GO

Create NonClustered Index IX_Contacts_Audit_missing_15 On [dbo].[Contacts_Audit] ([ContactID], [AuditAction]) Include ([LastUpdatedBy], [LastUpdatedOn]);
GO


Create NonClustered Index IX_Contacts_Audit_missing_9 On [dbo].[Contacts_Audit] ([ContactID]);
GO



CREATE CLUSTERED COLUMNSTORE INDEX [ClusteredColumnStoreIndex-Contacts_Audit] ON [dbo].[Contacts_Audit] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [AccountId_Scheme_UserActivityLogs]([AccountID])
