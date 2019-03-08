CREATE TABLE [dbo].[Contacts] (
    [ContactID]            INT              IDENTITY (1, 1) NOT NULL,
    [FirstName]            NVARCHAR (200)   NULL,
    [LastName]             NVARCHAR (200)   NULL,
    [Company]              NVARCHAR (200)   NULL,
    [CommunicationID]      INT              NULL,
    [Title]                NVARCHAR (300)   NULL,
    [ContactImageUrl]      VARCHAR (2000)   NULL,
    [AccountID]            INT              NOT NULL,
    [LeadSource]           NVARCHAR (100)   NULL,
    [HomePhone]            VARCHAR (75)     NULL,
    [WorkPhone]            VARCHAR (75)     NULL,
    [MobilePhone]          VARCHAR (75)     NULL,
    [PrimaryEmail]         VARCHAR (300)    NULL,
    [ContactType]          TINYINT          NOT NULL,
    [SSN]                  VARCHAR (300)    NULL,
    [LifecycleStage]       SMALLINT         NULL,
    [DoNotEmail]           BIT              NULL,
    [LastContacted]        DATETIME         NULL,
    [IsDeleted]            BIT              NULL,
    [ProfileImageKey]      UNIQUEIDENTIFIER NULL,
    [ImageID]              INT              NULL,
    [ReferenceID]          UNIQUEIDENTIFIER NULL,
    [LastUpdatedBy]        INT              NULL,
    [LastUpdatedOn]        DATETIME         NULL,
    [OwnerID]              INT              NULL,
    [PartnerType]          SMALLINT         NULL,
    [ContactSource]        TINYINT          NULL,
    [SourceType]           INT              NULL,
    [CompanyID]            INT              NULL,
    [LastContactedThrough] TINYINT          NULL,
    [FirstContactSource]   TINYINT          NULL,
    [FirstSourceType]      INT              NULL,
	[LeadScore] INT  NULL CONSTRAINT [Contacts_LeadScore]  DEFAULT ('0'),
    CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED ([ContactID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_Contacts_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Contacts_Communications] FOREIGN KEY ([CommunicationID]) REFERENCES [dbo].[Communications] ([CommunicationID]),
    CONSTRAINT [FK_Contacts_Contacts1] FOREIGN KEY ([CompanyID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_Contacts_DropdownValues] FOREIGN KEY ([LifecycleStage]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_Contacts_DropdownValues1] FOREIGN KEY ([PartnerType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID]),
    CONSTRAINT [FK_Contacts_Users] FOREIGN KEY ([OwnerID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [_dta_index_Contacts_6_1796201449__K20_K1]
    ON [dbo].[Contacts]([IsDeleted] ASC, [ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID]
    ON [dbo].[Contacts]([AccountID] ASC)
    INCLUDE([PrimaryEmail]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_CompanyID_ContactID]
    ON [dbo].[Contacts]([AccountID] ASC)
    INCLUDE([ContactID], [FirstName], [LastName], [Company], [CommunicationID], [Title], [ContactImageUrl], [LeadSource], [HomePhone], [WorkPhone], [MobilePhone], [PrimaryEmail], [ContactType], [SSN], [LifecycleStage], [DoNotEmail], [LastContacted], [IsDeleted], [ProfileImageKey], [ImageID], [ReferenceID], [LastUpdatedBy], [LastUpdatedOn], [OwnerID], [PartnerType], [ContactSource], [SourceType], [CompanyID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_ContactID_FirstName]
    ON [dbo].[Contacts]([AccountID] ASC)
    INCLUDE([ContactID], [FirstName], [LastName], [Company], [CommunicationID], [Title], [ContactImageUrl], [LeadSource], [HomePhone], [WorkPhone], [MobilePhone], [PrimaryEmail], [ContactType], [SSN], [LifecycleStage], [DoNotEmail], [LastContacted], [IsDeleted], [ProfileImageKey], [ImageID], [ReferenceID], [LastUpdatedBy], [LastUpdatedOn], [OwnerID], [PartnerType]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_ContactType]
    ON [dbo].[Contacts]([AccountID] ASC, [ContactType] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [ix_Contacts_AccountID_ContactType_Company]
    ON [dbo].[Contacts]([AccountID] ASC, [ContactType] ASC, [Company] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO



CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_IsDeleted]
    ON [dbo].[Contacts]([AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([OwnerID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_IsDeleted_CommunicationID]
    ON [dbo].[Contacts]([AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([FirstName], [LastName], [Company], [PrimaryEmail]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_IsDeleted_CommunicationID_CommunicationID]
    ON [dbo].[Contacts]([AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID], [FirstName], [LastName], [Company], [CommunicationID], [Title], [ContactImageUrl], [HomePhone], [WorkPhone], [MobilePhone], [PrimaryEmail], [ContactType], [SSN], [LifecycleStage], [DoNotEmail], [LastContacted], [ProfileImageKey], [ImageID], [ReferenceID], [LastUpdatedBy], [LastUpdatedOn], [OwnerID], [PartnerType], [ContactSource], [SourceType], [CompanyID], [LastContactedThrough], [FirstContactSource], [FirstSourceType]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_IsDeleted_CompanyID_ContactID]
    ON [dbo].[Contacts]([AccountID] ASC, [IsDeleted] ASC, [CompanyID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_IsDeleted_ContactID_FirstName]
    ON [dbo].[Contacts]([AccountID] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID], [FirstName], [LastName], [Company], [CommunicationID], [Title], [ContactImageUrl], [HomePhone], [WorkPhone], [MobilePhone], [PrimaryEmail], [ContactType], [SSN], [LifecycleStage], [DoNotEmail], [LastContacted], [ProfileImageKey], [ImageID], [ReferenceID], [LastUpdatedBy], [LastUpdatedOn], [OwnerID], [PartnerType], [ContactSource], [SourceType], [CompanyID], [LastContactedThrough]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_IsDeleted_FirstContactSource]
    ON [dbo].[Contacts]([AccountID] ASC, [IsDeleted] ASC, [FirstContactSource] ASC)
    INCLUDE([ContactID], [ReferenceID], [OwnerID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_AccountID_LifecycleStage_IsDeleted]
    ON [dbo].[Contacts]([AccountID] ASC, [LifecycleStage] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID], [FirstName], [LastName], [LastUpdatedOn], [OwnerID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_Company_AccountID_ContactType_ContactID]
    ON [dbo].[Contacts]([Company] ASC, [AccountID] ASC, [ContactType] ASC, [ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_ContactType_IsDeleted]
    ON [dbo].[Contacts]([ContactType] ASC, [IsDeleted] ASC)
    INCLUDE([ContactID], [Company], [AccountID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_ContactType_IsDeleted_CompanyID_ContactID]
    ON [dbo].[Contacts]([ContactType] ASC, [IsDeleted] ASC, [CompanyID] ASC, [ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_FirstName_LastName_AccountID_ContactType_ContactID]
    ON [dbo].[Contacts]([FirstName] ASC, [LastName] ASC, [AccountID] ASC, [ContactType] ASC, [ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_IsDeleted]
    ON [dbo].[Contacts]([IsDeleted] ASC)
    INCLUDE([ContactID], [AccountID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_IsDeleted_OwnerID]
    ON [dbo].[Contacts]([IsDeleted] ASC, [OwnerID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_LastName]
    ON [dbo].[Contacts]([LastName] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_LastUpdatedOn]
    ON [dbo].[Contacts]([LastUpdatedOn] ASC)
    INCLUDE([ContactID], [FirstName], [LastName], [Company], [CommunicationID], [Title], [ContactImageUrl], [AccountID], [LeadSource], [HomePhone], [WorkPhone], [MobilePhone], [PrimaryEmail], [ContactType], [SSN], [LifecycleStage], [DoNotEmail], [LastContacted], [IsDeleted], [ProfileImageKey], [ImageID], [ReferenceID], [LastUpdatedBy], [OwnerID], [PartnerType], [ContactSource], [SourceType], [CompanyID]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_LifecycleStage]
    ON [dbo].[Contacts]([LifecycleStage] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_OwnerID]
    ON [dbo].[Contacts]([OwnerID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO

CREATE NONCLUSTERED INDEX [IX_Contacts_PrimaryEmail]
    ON [dbo].[Contacts]([PrimaryEmail] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_Contacts_ReferenceID]
    ON [dbo].[Contacts]([ReferenceID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [missing_index_96]
    ON [dbo].[Contacts]([ContactType] ASC, [IsDeleted] ASC, [OwnerID] ASC, [ContactID] ASC) WITH (FILLFACTOR = 90)
    ON [SECONDARY];


GO

Create NonClustered Index IX_Contacts_missing_166 On [dbo].[Contacts] ([AccountID], [IsDeleted]);
GO

Create NonClustered Index IX_Contacts_missing_104 On [dbo].[Contacts] ([AccountID], [IsDeleted], [FirstContactSource]) Include ([ContactID]);
GO

Create NonClustered Index IX_Contacts_missing_5 On [dbo].[Contacts] ([AccountID], [IsDeleted],[LastUpdatedOn]) Include ([ContactID], [LifecycleStage], [OwnerID]);

GO

Create NonClustered Index IX_Contacts_missing_7 On [dbo].[Contacts] ([AccountID], [IsDeleted]) Include ([ContactID]);
GO
