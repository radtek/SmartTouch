
CREATE FUNCTION [dbo].[GenerateContactSummary] 
	(
		@ContactID int
	)
	RETURNS		nvarchar(max)
AS
BEGIN
	DECLARE @ContactSummary nvarchar(max)

	select @ContactSummary = '['+ STUFF((
        select 
            ',{"CreatedOn":"' + CONVERT(varchar(23), n.CreatedOn, 121)
            + '","NoteDetails":"' + replace(n.NoteDetails,'"','\"') + '"'
            + ',"CreatedBy":"' + cast(u.FirstName +' ' +u.LastName as varchar(50)) + '"'
			+ ',"NoteCategory":"' +cast(d.DropdownValue  as varchar(50))  + '"}'
			from notes n
		left join contactnotemap cnm on n.noteid = cnm.NoteID 
		join Users u on n.CreatedBy = u.UserId
		join DropdownValues d on d.DropdownValueID = n.NoteCategory
		where n.AddToContactSummary = 1 and cnm.ContactID =@ContactID 
        for xml path(''), type
    ).value('.', 'varchar(max)'), 1, 1, '') +']'

	
	RETURN @ContactSummary
END


GO


