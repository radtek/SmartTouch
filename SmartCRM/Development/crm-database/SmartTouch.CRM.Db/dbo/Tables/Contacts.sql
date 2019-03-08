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


CREATE NONCLUSTERED INDEX [IX_Contacts_LastUpdatedOn] ON [dbo].[Contacts]
(
	[LastUpdatedOn] ASC
)
INCLUDE ( 	[ContactID],
	[FirstName],
	[LastName],
	[Company],
	[CommunicationID],
	[Title],
	[ContactImageUrl],
	[AccountID],
	[LeadSource],
	[HomePhone],
	[WorkPhone],
	[MobilePhone],
	[PrimaryEmail],
	[ContactType],
	[SSN],
	[LifecycleStage],
	[DoNotEmail],
	[LastContacted],
	[IsDeleted],
	[ProfileImageKey],
	[ImageID],
	[ReferenceID],
	[LastUpdatedBy],
	[OwnerID],
	[PartnerType],
	[ContactSource],
	[SourceType],
	[CompanyID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO


CREATE NONCLUSTERED INDEX [IX_Contacts_missing_5] ON [dbo].[Contacts]
(
	[AccountID] ASC,
	[IsDeleted] ASC,
	[LastUpdatedOn] ASC
)
INCLUDE ( 	[ContactID],
	[LifecycleStage],
	[OwnerID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)
GO