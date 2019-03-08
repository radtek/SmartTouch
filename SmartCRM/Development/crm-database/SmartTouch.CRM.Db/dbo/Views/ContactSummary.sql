
CREATE VIEW [dbo].[ContactSummary]
AS
select	cnm.ContactID, 
		(select dbo.GenerateContactSummary(cnm.contactid)) as ContactSummary, 
		(select dbo.GenerateConsolidatedNoteDetails(cnm.contactid)) as NoteDetails,
		lni.NoteDetails as LastNote, 
		lni.CreatedOn as LastNoteDate, 
		lni.CreatedBy as LastNoteCreatedBy,
		lni.NoteCategory as NoteCategory
from ContactNoteMap cnm
	inner join notes n on cnm.NoteID = n.noteid 
	inner join users u on n.createdby = u.UserID
	inner join GetContactLastNoteInfo lni on cnm.Contactid = lni.Contactid
where n.AddToContactSummary = 1 and lni.SortOrder = 1








GO


