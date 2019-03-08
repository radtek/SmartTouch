
CREATE TYPE [dbo].[AddresseType] AS TABLE(
	[AddressID] [int] NULL,
	[AddressTypeID] [smallint] NULL,
	[AddressLine1] [varchar](75) NULL,
	[AddressLine2] [varchar](75) NULL,
	[City] [varchar](75) NULL,
	[StateID] [varchar](75) NULL,
	[CountryID] [nchar](75) NULL,
	[ZipCode] [varchar](75) NULL,
	[IsDefault] [bit] NULL
)
GO


