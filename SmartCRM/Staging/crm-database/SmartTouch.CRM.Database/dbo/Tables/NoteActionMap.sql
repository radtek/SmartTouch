CREATE TABLE [dbo].[NoteActionMap] (
    [NoteActionMapID] INT IDENTITY (1, 1) NOT NULL,
    [ActionID]        INT NOT NULL,
    [NoteID]          INT NOT NULL,
    PRIMARY KEY CLUSTERED ([NoteActionMapID] ASC)
);

