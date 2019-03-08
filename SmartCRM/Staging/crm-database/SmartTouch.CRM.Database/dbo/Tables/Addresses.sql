CREATE TABLE [dbo].[Addresses] (
    [AddressID]     INT          IDENTITY (1, 1) NOT NULL,
    [AddressTypeID] SMALLINT     NOT NULL,
    [AddressLine1]  VARCHAR (95) NULL,
    [AddressLine2]  VARCHAR (95) NULL,
    [City]          VARCHAR (35) NULL,
    [StateID]       VARCHAR (50) NULL,
    [CountryID]     NCHAR (2)    NULL,
    [ZipCode]       VARCHAR (11) NULL,
    [IsDefault]     BIT          NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([AddressID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON)
);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_IsDefault]
    ON [dbo].[Addresses]([IsDefault] ASC)
    INCLUDE([AddressID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

