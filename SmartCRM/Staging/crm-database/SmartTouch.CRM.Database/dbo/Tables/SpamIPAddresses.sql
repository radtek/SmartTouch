CREATE TABLE [dbo].[SpamIPAddresses] (
    [SpamIPAddressID] INT        IDENTITY (1, 1) NOT NULL,
    [IsSpam]          BIT        NOT NULL,
    [AccountID]       INT        NULL,
    [IPAddress]       BINARY (4) NULL,
    CONSTRAINT [PK_SpamIPAddresses] PRIMARY KEY CLUSTERED ([SpamIPAddressID] ASC) WITH (FILLFACTOR = 90)
);

