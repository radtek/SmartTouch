

CREATE PROCEDURE [dbo].[DeleteNote]
	@noteId int
AS
	DECLARE @relatedContactIds TABLE (Id INT)
	INSERT INTO @relatedContactIds SELECT DISTINCT ContactId FROM ContactNoteMap (NOLOCK) WHERE noteid = @noteId
	
	DELETE ContactNoteMap WHERE Noteid = @noteId 
	DELETE OpportunityNoteMap WHERE Noteid = @noteId
	DELETE NoteTagMap WHERE Noteid = @noteId 
	DELETE Notes WHERE Noteid = @noteId
	
	SELECT * FROM @relatedContactIds

