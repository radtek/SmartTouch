

CREATE FUNCTION [dbo].[GenerateConsolidatedNoteDetails] 
	(
		@ContactID int
	)
	RETURNS		nvarchar(max)
AS
BEGIN
	DECLARE @ContactSummary nvarchar(max)

	select @ContactSummary = STUFF((
        select 
            ' || ' + cast(n.NoteDetails as nvarchar(1000))
			from contactnotemap cnm
		inner join notes n  on n.noteid = cnm.NoteID 
		where n.AddToContactSummary = 1 and cnm.ContactID = @ContactID
		  for xml path(''), type
  ).value('.', 'varchar(max)'), 1, 3, '')

	RETURN @ContactSummary
END



