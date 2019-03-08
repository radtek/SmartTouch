CREATE view [dbo].[GetContactLastNoteInfo]
as
select * from ( select	cnm.ContactID, n.NoteId, n.NoteDetails, n.CreatedBy, n.CreatedOn,n.NoteCategory,
		RANK() OVER(PARTITION BY cnm.contactid ORDER BY n.noteid DESC) SortOrder
from	Notes n inner join ContactNoteMap cnm on n.NoteID = cnm.NoteID
		inner join users u on n.CreatedBy = u.UserID
		inner join DropdownValues d on d.DropdownValueID = n.NoteCategory
where	n.AddToContactSummary = 1 ) ln where SortOrder = 1


GO


