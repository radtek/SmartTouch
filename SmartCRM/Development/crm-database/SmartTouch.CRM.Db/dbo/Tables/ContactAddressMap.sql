CREATE TABLE [dbo].[ContactAddressMap] (
    [ContactAddressMapID] INT IDENTITY (1, 1) NOT NULL,
    [ContactID]           INT NULL,
    [AddressID]           INT NULL,
    CONSTRAINT [PK_ContactAddressMap] PRIMARY KEY CLUSTERED ([ContactAddressMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactAddressMap_Addresses] FOREIGN KEY ([AddressID]) REFERENCES [dbo].[Addresses] ([AddressID]),
    CONSTRAINT [FK_ContactAddressMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactAddressMap_AddressID]
    ON [dbo].[ContactAddressMap]([AddressID] ASC)
    INCLUDE([ContactAddressMapID], [ContactID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactAddressMap_ContactID]
    ON [dbo].[ContactAddressMap]([ContactID] ASC)
    INCLUDE([AddressID]) WITH (FILLFACTOR = 90);

