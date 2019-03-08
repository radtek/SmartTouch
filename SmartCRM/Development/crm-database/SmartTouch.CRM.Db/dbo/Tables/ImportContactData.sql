CREATE TABLE [dbo].[ImportContactData] (
   [ImportContactDataID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](4000) NULL,
	[LastName] [nvarchar](4000) NULL,
	[CompanyName] [nvarchar](4000) NULL,
	[Title] [nvarchar](300) NULL,
	[LeadSource] [nvarchar](100) NULL,
	[LifecycleStage] [nvarchar](50) NULL,
	[PartnerType] [nvarchar](50) NULL,
	[DoNotEmail] [bit] NULL,
	[HomePhone] [nvarchar](20) NULL,
	[MobilePhone] [nvarchar](20) NULL,
	[WorkPhone] [nvarchar](20) NULL,
	[AccountID] [int] NULL,
	[PrimaryEmail] [nvarchar](256) NULL,
	[SecondaryEmails] [nvarchar](4000) NULL,
	[FacebookUrl] [nvarchar](4000) NULL,
	[TwitterUrl] [nvarchar](4000) NULL,
	[GooglePlusUrl] [nvarchar](4000) NULL,
	[LinkedInUrl] [nvarchar](4000) NULL,
	[BlogUrl] [nvarchar](4000) NULL,
	[WebSiteUrl] [nvarchar](4000) NULL,
	[AddressLine1] [nvarchar](95) NULL,
	[AddressLine2] [nvarchar](95) NULL,
	[City] [nvarchar](35) NULL,
	[State] [nvarchar](65) NULL,
	[Country] [nvarchar](65) NULL,
	[ZipCode] [nvarchar](11) NULL,
	[CustomFieldsData] [nvarchar](4000) NULL,
	[ContactID] [int] NULL,
	[ContactStatusID] [tinyint] NULL,
	[ReferenceID] [uniqueidentifier] NULL,
	[ContactTypeID] [tinyint] NULL,
	[OwnerID] [int] NULL,
	[JobID] [int] NULL,
	[LeadSourceID] [smallint] NULL,
	[LifecycleStageID] [smallint] NULL,
	[PartnerTypeID] [smallint] NULL,
	[LoopID] [int] NULL,
	[CompanyID] [int] NULL,
	[PhoneData] [nvarchar](4000) NULL,
	[CommunicationID] [int] NULL,
	[EmailExists] [bit] NULL,
	[IsBuilderNumberPass] [bit] NULL,
	[LeadAdapterSubmittedData] [nvarchar](4000) NULL,
	[LeadAdapterRowData] [nvarchar](4000) NULL,
	[ValidEmail] [bit] NULL,
	[OrginalRefId] [varchar](50) NULL,
	[IsDuplicate] [bit] NOT NULL,
	[IsCommunityNumberPass] [bit] NULL,
    CONSTRAINT [PK_ImportContactData] PRIMARY KEY CLUSTERED ([ImportContactDataID] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_AccountID_JobID_ValidEmail_IsDuplicate_ContactID_ContactStatusID_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([AccountID] ASC, [JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [ContactStatusID] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_AccountID_JobID_ValidEmail_IsDuplicate_ContactID_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([AccountID] ASC, [JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_ImportContactData_ContactID]
    ON [dbo].[ImportContactData]([ContactID] ASC)
    INCLUDE([ImportContactDataID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_ContactStatusID_JobID_IsDuplicate]
    ON [dbo].[ImportContactData]([ContactStatusID] ASC, [JobID] ASC, [IsDuplicate] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_IsDuplicate]
    ON [dbo].[ImportContactData]([JobID] ASC, [IsDuplicate] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_IsDuplicate_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([JobID] ASC, [IsDuplicate] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_ValidEmail_CompanyID_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([JobID] ASC, [ValidEmail] ASC, [CompanyID] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_ValidEmail_ContactID_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([JobID] ASC, [ValidEmail] ASC, [ContactID] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_ValidEmail_IsDuplicate]
    ON [dbo].[ImportContactData]([JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_ValidEmail_IsDuplicate_ContactID_ContactStatusID_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [ContactStatusID] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_ValidEmail_IsDuplicate_ContactID_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_JobID_ValidEmail_IsDuplicate_IsBuilderNumberPass]
    ON [dbo].[ImportContactData]([JobID] ASC, [ValidEmail] ASC, [IsDuplicate] ASC, [IsBuilderNumberPass] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ImportContactData_PrimaryEmail_IsDuplicate]
    ON [dbo].[ImportContactData]([PrimaryEmail] ASC, [IsDuplicate] ASC)
    INCLUDE([ImportContactDataID], [ContactID], [ContactStatusID]) WITH (FILLFACTOR = 90);


GO

CREATE NONCLUSTERED INDEX [IX_ImportContactData_ValidEmail_IsDuplicate_ContactID_IsBuilderNumberPass_IsCommunityNumberPass]
    ON [dbo].[ImportContactData]([ValidEmail] ASC, [IsDuplicate] ASC, [ContactID] ASC, [IsBuilderNumberPass] ASC, [IsCommunityNumberPass] ASC)
    INCLUDE([OwnerID], [JobID]) WITH (FILLFACTOR = 90);

GO

CREATE NONCLUSTERED INDEX [IDX_IMportContactdata_referenceID]
ON [dbo].[ImportContactData] ([ReferenceID])

GO

CREATE NONCLUSTERED INDEX [IX_ImportContactData_missing_371] ON [dbo].[ImportContactData]
(
	[AccountID] ASC,
	[JobID] ASC,
	[ValidEmail] ASC,
	[IsDuplicate] ASC,
	[ContactID] ASC,
	[IsBuilderNumberPass] ASC,
	[IsCommunityNumberPass] ASC
)
INCLUDE ( 	[ImportContactDataID],
	[OrginalRefId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ImportContactData_missing_385] ON [dbo].[ImportContactData]
(
	[AccountID] ASC,
	[JobID] ASC,
	[ValidEmail] ASC,
	[IsDuplicate] ASC,
	[ContactID] ASC,
	[ContactStatusID] ASC,
	[IsBuilderNumberPass] ASC,
	[IsCommunityNumberPass] ASC
)
INCLUDE ( 	[OrginalRefId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

