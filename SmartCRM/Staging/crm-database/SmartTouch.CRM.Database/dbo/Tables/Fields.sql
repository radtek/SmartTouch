CREATE TABLE [dbo].[Fields] (
    [FieldID]              INT            IDENTITY (1, 1) NOT NULL,
    [Title]                NVARCHAR (75)  NOT NULL,
    [FieldInputTypeID]     TINYINT        NOT NULL,
    [ValidationMessage]    NVARCHAR (200) NULL,
    [ParentID]             INT            NULL,
    [AccountID]            INT            NULL,
    [FieldCode]            NVARCHAR (75)  NOT NULL,
    [SortID]               TINYINT        NULL,
    [CustomFieldSectionID] INT            NULL,
    [StatusID]             SMALLINT       NULL,
    [IsLeadAdapterField]   BIT            NOT NULL,
    [LeadAdapterType]      TINYINT        NULL,
    CONSTRAINT [PK_Fields] PRIMARY KEY CLUSTERED ([FieldID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Fields_Accounts] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts] ([AccountID]),
    CONSTRAINT [FK_Fields_CustomFieldSections] FOREIGN KEY ([CustomFieldSectionID]) REFERENCES [dbo].[CustomFieldSections] ([CustomFieldSectionID]),
    CONSTRAINT [FK_Fields_FieldInputTypes] FOREIGN KEY ([FieldInputTypeID]) REFERENCES [dbo].[FieldInputTypes] ([FieldInputTypeID]),
    CONSTRAINT [FK_Fields_Statuses] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[Statuses] ([StatusID])
);

