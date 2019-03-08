CREATE TYPE [dbo].[AddresseType] AS TABLE (
    [AddressID]     INT          NULL,
    [AddressTypeID] SMALLINT     NULL,
    [AddressLine1]  VARCHAR (95) NULL,
    [AddressLine2]  VARCHAR (95) NULL,
    [City]          VARCHAR (35) NULL,
    [StateID]       VARCHAR (50) NULL,
    [CountryID]     NCHAR (2)    NULL,
    [ZipCode]       VARCHAR (11) NULL,
    [IsDefault]     BIT          NULL);

