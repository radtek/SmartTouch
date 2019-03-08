CREATE TABLE [dbo].[LeadAdapterCustomFields] (
    [LeadAdapterCustomFieldID] SMALLINT      IDENTITY (1, 1) NOT NULL,
    [Title]                    NVARCHAR (75) NOT NULL,
    [FieldInputTypeID]         TINYINT       NOT NULL,
    [SortID]                   TINYINT       NOT NULL,
    [LeadAdapterType]          TINYINT       NULL,
    CONSTRAINT [PK_LeadAdapterCustomFields] PRIMARY KEY CLUSTERED ([LeadAdapterCustomFieldID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_LeadAdapterCustomFields_FieldInputTypes] FOREIGN KEY ([FieldInputTypeID]) REFERENCES [dbo].[FieldInputTypes] ([FieldInputTypeID]),
    CONSTRAINT [FK_LeadAdapterCustomFields_LeadAdapterTypes] FOREIGN KEY ([LeadAdapterType]) REFERENCES [dbo].[LeadAdapterTypes] ([LeadAdapterTypeID])
);

