CREATE TABLE [dbo].[CustomFieldValueOptions] (
    [CustomFieldValueOptionID] INT             IDENTITY (1, 1) NOT NULL,
    [CustomFieldID]            INT             NOT NULL,
    [Value]                    NVARCHAR (4000) NULL,
    [IsDeleted]                BIT             NOT NULL,
	[Order]                    INT         NOT NULL,
    CONSTRAINT [PK_CustomFieldValueOptions] PRIMARY KEY CLUSTERED ([CustomFieldValueOptionID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CustomFieldValueOptions_Fields] FOREIGN KEY ([CustomFieldID]) REFERENCES [dbo].[Fields] ([FieldID])
);


GO
CREATE NONCLUSTERED INDEX [IX_CustomFieldValueOptions_CustomFieldID]
    ON [dbo].[CustomFieldValueOptions]([CustomFieldID] ASC)
    INCLUDE([CustomFieldValueOptionID], [Value], [IsDeleted]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_CustomFieldValueOptions_CustomFieldID_IsDeleted]
    ON [dbo].[CustomFieldValueOptions]([CustomFieldID] ASC, [IsDeleted] ASC)
    INCLUDE([CustomFieldValueOptionID], [Value]) WITH (FILLFACTOR = 90);

