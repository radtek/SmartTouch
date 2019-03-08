CREATE TABLE [dbo].[ContactIPAddresses] (
    [ContactIPAddressID] INT            IDENTITY (1, 1) NOT NULL,
    [ContactID]          INT            NOT NULL,
    [IPAddress]          VARCHAR (45)   NOT NULL,
    [IdentifiedOn]       DATETIME       NOT NULL,
    [City]               NVARCHAR (100) NULL,
    [Region]             NVARCHAR (100) NULL,
    [Country]            NVARCHAR (100) NULL,
    [ISPName]            VARCHAR (100)  NULL,
    [UniqueTrackingID]   NVARCHAR (50)  NULL,
    CONSTRAINT [PK_ContactIPAddresses] PRIMARY KEY CLUSTERED ([ContactIPAddressID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactIPAddresses_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactIPAddresses_ContactID_UniqueTrackingID]
    ON [dbo].[ContactIPAddresses]([ContactID] ASC, [UniqueTrackingID] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactIPAddresses_UniqueTrackingID]
    ON [dbo].[ContactIPAddresses]([UniqueTrackingID] ASC)
    INCLUDE([ContactID]) WITH (FILLFACTOR = 90);

