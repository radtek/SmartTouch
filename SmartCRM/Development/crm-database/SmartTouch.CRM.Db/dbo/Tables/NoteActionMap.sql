CREATE TABLE [dbo].[NoteActionMap] (
    [NoteActionMapID] INT IDENTITY (1, 1) NOT NULL,
    [ActionID]        INT NOT NULL,
    [NoteID]          INT NOT NULL,
    CONSTRAINT [PK_NoteActionMap] PRIMARY KEY CLUSTERED ([NoteActionMapID] ASC) WITH (FILLFACTOR = 90)
);
GO