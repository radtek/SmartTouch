CREATE TABLE [dbo].[FieldInputTypes] (
    [FieldInputTypeID] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Type]             NVARCHAR (75) NOT NULL,
    CONSTRAINT [PK_FieldInputTypes] PRIMARY KEY CLUSTERED ([FieldInputTypeID] ASC) WITH (FILLFACTOR = 90)
);

