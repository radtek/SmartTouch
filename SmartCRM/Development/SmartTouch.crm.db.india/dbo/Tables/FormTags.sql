CREATE TABLE [dbo].[FormTags] (
    [FormTagID] INT IDENTITY (1, 1) NOT NULL,
    [FormID]    INT NOT NULL,
    [TagID]     INT NOT NULL,
    CONSTRAINT [PK_FormTags] PRIMARY KEY CLUSTERED ([FormTagID] ASC) WITH (FILLFACTOR = 90) ON [SECONDARY],
    CONSTRAINT [FK_FormTags_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID]),
    CONSTRAINT [FK_FormTags_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID])
);

