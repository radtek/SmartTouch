



CREATE view [dbo].[GetContactLastNoteInfo]
as
select * from ( select	cnm.ContactID, n.NoteId, n.NoteDetails, n.CreatedBy, n.CreatedOn,n.NoteCategory,
		RANK() OVER(PARTITION BY cnm.contactid ORDER BY n.noteid DESC) SortOrder
from	Notes n inner join ContactNoteMap cnm WITH (NOLOCK) on n.NoteID = cnm.NoteID
		inner join users u WITH (NOLOCK) on n.CreatedBy = u.UserID
		inner join DropdownValues d WITH (NOLOCK) on d.DropdownValueID = n.NoteCategory
where	n.AddToContactSummary = 1 ) ln where SortOrder = 1



