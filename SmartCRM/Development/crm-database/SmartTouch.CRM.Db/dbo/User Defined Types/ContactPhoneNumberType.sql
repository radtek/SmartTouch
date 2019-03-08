
CREATE TYPE [dbo].[ContactPhoneNumberType] AS TABLE(
	[ContactPhoneNumberID] [int] NULL,
	[ContactID] [int] NULL,
	[Number] [varchar](50) NULL,
	[PhoneType] [smallint] NULL,
	[IsPrimary] [bit] NULL,
	[AccountID] [int] NULL,
	[IsDeleted] [bit] NULL,
	[CountryCode] [varchar](3) NULL,
	[Extension] [varchar](5) NULL
)
GO


