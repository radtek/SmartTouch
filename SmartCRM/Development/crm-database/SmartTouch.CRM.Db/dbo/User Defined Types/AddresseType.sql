
CREATE TYPE [dbo].[AddresseType] AS TABLE(
            [AddressID] [int] NULL,
            [AddressTypeID] [smallint] NULL,
            [AddressLine1] [varchar](95) NULL,
            [AddressLine2] [varchar](95) NULL,
            [City] [varchar](35) NULL,
            [StateID] [varchar](50) NULL,
            [CountryID] [nchar](2) NULL,
            [ZipCode] [varchar](11) NULL,
            [IsDefault] [bit] NULL
)
GO