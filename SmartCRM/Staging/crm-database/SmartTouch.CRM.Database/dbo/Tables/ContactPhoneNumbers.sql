CREATE TABLE [dbo].[ContactPhoneNumbers] (
    [ContactPhoneNumberID] INT          IDENTITY (1, 1) NOT NULL,
    [ContactID]            INT          NOT NULL,
    [PhoneNumber]          VARCHAR (50) NOT NULL,
    [PhoneType]            SMALLINT     NOT NULL,
    [IsPrimary]            BIT          NOT NULL,
    [AccountID]            INT          NOT NULL,
    [IsDeleted]            BIT          NOT NULL,
    [CountryCode]          VARCHAR (3)  NULL,
    [Extension]            VARCHAR (5)  NULL,
    CONSTRAINT [PK_ContactPhoneNumbers] PRIMARY KEY CLUSTERED ([ContactPhoneNumberID] ASC) WITH (FILLFACTOR = 90, PAD_INDEX = ON),
    CONSTRAINT [FK_ContactPhoneNumbers_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_ContactPhoneNumbers_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactPhoneNumbers_DropdownValues] FOREIGN KEY ([PhoneType]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactPhoneNumbers_missing_58427]
    ON [dbo].[ContactPhoneNumbers]([IsDeleted] ASC)
    INCLUDE([ContactID], [PhoneType]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactPhoneNumbers_ContactID]
    ON [dbo].[ContactPhoneNumbers]([ContactID] ASC)
    INCLUDE([ContactPhoneNumberID], [PhoneNumber], [PhoneType], [IsPrimary], [AccountID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);


GO
CREATE NONCLUSTERED INDEX [IX_ContactPhoneNumbers_IsPrimary_IsDeleted]
    ON [dbo].[ContactPhoneNumbers]([IsPrimary] ASC, [IsDeleted] ASC)
    INCLUDE([ContactPhoneNumberID], [ContactID], [PhoneNumber], [PhoneType], [AccountID]) WITH (FILLFACTOR = 90, PAD_INDEX = ON);

