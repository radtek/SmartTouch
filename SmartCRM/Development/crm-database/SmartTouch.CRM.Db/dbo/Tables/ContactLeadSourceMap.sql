CREATE TABLE [dbo].[ContactLeadSourceMap] (
    [ContactLeadSourceMapID] INT      IDENTITY (1, 1) NOT NULL,
    [ContactID]              INT      NOT NULL,
    [LeadSouceID]            SMALLINT NOT NULL,
    [IsPrimaryLeadSource]    BIT      NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL CONSTRAINT [df_lastupdateddate]  DEFAULT (getutcdate()),
    CONSTRAINT [PK_ContactLeadSourceMap] PRIMARY KEY CLUSTERED ([ContactLeadSourceMapID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_ContactLeadSourceMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactLeadSourceMap_DropdownValues] FOREIGN KEY ([LeadSouceID]) REFERENCES [dbo].[DropdownValues] ([DropdownValueID])
);


GO



GO
CREATE NONCLUSTERED INDEX [IX_ContactLeadSourceMap_ContactID_IsPrimaryLeadSource]
    ON [dbo].[ContactLeadSourceMap]([ContactID] ASC, [IsPrimaryLeadSource] ASC)
    INCLUDE([LeadSouceID]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_ContactLeadSourceMap_ContactID1]
    ON [dbo].[ContactLeadSourceMap]([ContactID] ASC)
    INCLUDE([ContactLeadSourceMapID], [LeadSouceID], [IsPrimaryLeadSource]) WITH (FILLFACTOR = 90);


GO



GO
CREATE NONCLUSTERED INDEX [IX_ContactLeadSourceMap_LeadSouceID_IsPrimaryLeadSource]
    ON [dbo].[ContactLeadSourceMap]([LeadSouceID] ASC, [IsPrimaryLeadSource] ASC)
    INCLUDE([ContactLeadSourceMapID], [ContactID]) WITH (FILLFACTOR = 90);

