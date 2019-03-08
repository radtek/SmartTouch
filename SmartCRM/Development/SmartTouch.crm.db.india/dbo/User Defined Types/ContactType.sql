
CREATE TYPE [dbo].[ContactType] AS TABLE(
	[ContactID] [int] NULL,
	[FirstName] [nvarchar](200) NULL,
	[LastName] [nvarchar](200) NULL,
	[Company] [nvarchar](200) NULL,
	[CommunicationID] [int] NULL,
	[Title] [nvarchar](300) NULL,
	[ContactImageUrl] [varchar](2000) NULL,
	[AccountID] [int] NULL,
	[LeadSource] [nvarchar](100) NULL,
	[HomePhone] [varchar](75) NULL,
	[WorkPhone] [varchar](75) NULL,
	[MobilePhone] [varchar](75) NULL,
	[PrimaryEmail] [varchar](300) NULL,
	[ContactType] [tinyint] NULL,
	[SSN] [varchar](300) NULL,
	[LifecycleStage] [smallint] NULL,
	[DoNotEmail] [bit] NULL,
	[LastContacted] [datetime] NULL,
	[IsDeleted] [bit] NULL,
	[ProfileImageKey] [uniqueidentifier] NULL,
	[ImageID] [int] NULL,
	[ReferenceID] [uniqueidentifier] NULL,
	[LastUpdatedBy] [int] NULL,
	[LastUpdatedOn] [datetime] NULL,
	[OwnerID] [int] NULL,
	[PartnerType] [smallint] NULL,
	[ContactSource] [tinyint] NULL,
	[SourceType] [int] NULL,
	[CompanyID] [int] NULL,
	[LastContactedThrough] [tinyint] NULL,
	[FirstContactSource] [tinyint] NULL,
	[FirstSourceType] [int] NULL,
	[LeadScore] [int] NULL
)
GO


