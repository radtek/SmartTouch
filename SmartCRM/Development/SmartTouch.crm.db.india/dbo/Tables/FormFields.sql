CREATE TABLE [dbo].[FormFields] (
    [FormFieldID] INT           IDENTITY (1, 1) NOT NULL,
    [FormID]      INT           NOT NULL,
    [FieldID]     INT           NOT NULL,
    [DisplayName] NVARCHAR (75) NOT NULL,
    [IsMandatory] BIT           NOT NULL,
    [SortID]      TINYINT       NOT NULL,
    [IsDeleted]   BIT           NULL,
    [IsHidden]    BIT           NOT NULL,
    CONSTRAINT [PK_FormFields] PRIMARY KEY CLUSTERED ([FormFieldID] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_FormFields_Fields] FOREIGN KEY ([FieldID]) REFERENCES [dbo].[Fields] ([FieldID]),
    CONSTRAINT [FK_FormFields_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID])
);


GO
CREATE NONCLUSTERED INDEX [IX_FormFields_FormID]
    ON [dbo].[FormFields]([FormID] ASC)
    INCLUDE([FormFieldID], [FieldID], [DisplayName], [IsMandatory], [SortID], [IsDeleted], [IsHidden]) WITH (FILLFACTOR = 90);

