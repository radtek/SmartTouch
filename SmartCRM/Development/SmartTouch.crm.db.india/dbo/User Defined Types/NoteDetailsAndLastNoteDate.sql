
CREATE TYPE [dbo].[NoteDetailsAndLastNoteDate] AS TABLE(
	[NoteDetails] [nvarchar](max) NULL,
	[LastNoteDate] [datetime] NULL
)
GO