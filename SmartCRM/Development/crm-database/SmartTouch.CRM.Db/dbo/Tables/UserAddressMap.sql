CREATE TABLE [dbo].[UserAddressMap] (
    [UserAddressMapID] INT IDENTITY (1, 1) NOT NULL,
    [UserID]           INT NOT NULL,
    [AddressID]        INT NOT NULL,
    CONSTRAINT [PK_UserAddressMap] PRIMARY KEY CLUSTERED ([UserAddressMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_UserAddressMap_Addresses] FOREIGN KEY ([AddressID]) REFERENCES [dbo].[Addresses] ([AddressID]),
    CONSTRAINT [FK_UserAddressMap_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

