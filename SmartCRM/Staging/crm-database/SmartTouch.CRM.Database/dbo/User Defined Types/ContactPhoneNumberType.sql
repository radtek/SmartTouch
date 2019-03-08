CREATE TYPE [dbo].[ContactPhoneNumberType] AS TABLE (
    [ContactPhoneNumberID] INT          NULL,
    [ContactID]            INT          NULL,
    [Number]               VARCHAR (50) NULL,
    [PhoneType]            SMALLINT     NULL,
    [IsPrimary]            BIT          NULL,
    [AccountID]            INT          NULL,
    [IsDeleted]            BIT          NULL,
    [CountryCode]          VARCHAR (3)  NULL,
    [Extension]            VARCHAR (5)  NULL);

