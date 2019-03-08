CREATE TABLE [dbo].[ContactLeadAdapterMap] (
    [ContactLeadAdapterMapID] INT     IDENTITY (1, 1) NOT NULL,
    [ContactID]               INT     NOT NULL,
    [LeadAdapterID]           TINYINT NOT NULL,
    CONSTRAINT [PK_ContactLeadAdapterMap] PRIMARY KEY CLUSTERED ([ContactLeadAdapterMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ContactLeadAdapterMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactLeadAdapterMap_LeadAdapterTypes] FOREIGN KEY ([LeadAdapterID]) REFERENCES [dbo].[LeadAdapterTypes] ([LeadAdapterTypeID])
);

