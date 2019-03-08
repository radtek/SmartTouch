CREATE TABLE [dbo].[ContactCustomFieldMap] (
    [ContactCustomFieldMapID] INT             IDENTITY (1, 1) NOT NULL,
    [ContactID]               INT             NOT NULL,
    [CustomFieldID]           INT             NOT NULL,
    [Value]                   NVARCHAR (4000) NULL,
    CONSTRAINT [PK_ContactCustomFieldMap] PRIMARY KEY CLUSTERED ([ContactCustomFieldMapID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_ContactCustomFieldMap_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_ContactCustomFieldMap_Fields] FOREIGN KEY ([CustomFieldID]) REFERENCES [dbo].[Fields] ([FieldID])
);


GO
CREATE NONCLUSTERED INDEX [IX_ContactCustomFieldMap_ContactID]
    ON [dbo].[ContactCustomFieldMap]([ContactID] ASC)
    INCLUDE([ContactCustomFieldMapID], [CustomFieldID], [Value]) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO

CREATE NONCLUSTERED INDEX [IX_ContactCustomFieldMap_ContactID_CustomFieldID]
    ON [dbo].[ContactCustomFieldMap]([ContactID] ASC, [CustomFieldID] ASC) WITH (FILLFACTOR = 90)
    ON [PRIMARY];


GO


