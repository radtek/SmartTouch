CREATE TABLE [dbo].[AccountAddressMap] (
    [AccountAddressMapID] INT IDENTITY (1, 1) NOT NULL,
    [AccountID]           INT NOT NULL,
    [AddressID]           INT NOT NULL,
    CONSTRAINT [PK_AccountAddressMap] PRIMARY KEY CLUSTERED ([AccountAddressMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_AccountAddressMap_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_AccountAddressMap_Addresses] FOREIGN KEY ([AddressID]) REFERENCES [dbo].[Addresses] ([AddressID])
);

